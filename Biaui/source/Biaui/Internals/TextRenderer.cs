﻿using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using Jewelry.Collections;

namespace Biaui.Internals
{
    internal partial class TextRenderer
    {
        internal static readonly TextRenderer Default;

        static TextRenderer()
        {
            var fontFamily = (FontFamily) Application.Current.FindResource("BiauiFontFamily");
            var fontSize = (double) TextElement.FontSizeProperty.DefaultMetadata.DefaultValue;

            Default = new TextRenderer(
                true,
                fontFamily, fontSize,
                FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
        }

        internal TextRenderer(
            bool isDefault,
            FontFamily fontFamily,
            double fontSize,
            FontStyle style,
            FontWeight weight,
            FontStretch stretch)
        {
            if (fontFamily == null)
                return;

            var typeface = new Typeface(fontFamily, style, weight, stretch);

            if (typeface.TryGetGlyphTypeface(out _glyphTypeface) == false)
            {
                // エラーの場合はデフォルトで作り直す
                typeface =
                    new Typeface(
                        (FontFamily) TextElement.FontFamilyProperty.DefaultMetadata.DefaultValue,
                        (FontStyle) TextElement.FontStyleProperty.DefaultMetadata.DefaultValue,
                        (FontWeight) TextElement.FontWeightProperty.DefaultMetadata.DefaultValue,
                        (FontStretch) TextElement.FontStretchProperty.DefaultMetadata.DefaultValue);

                // デフォルトでもだめなら以降処理しない
                if (typeface.TryGetGlyphTypeface(out _glyphTypeface) == false)
                    return;
            }

            _isDefault = isDefault;
            _fontSize = fontSize;

            if (_isDefault)
            {
                _fontLineSpacing = DefaultFontLineSpacing;

                _dotGlyphIndex = Unsafe.Add(ref GetDefaultGlyphIndexTable(), (IntPtr) '.');
                _dotAdvanceWidth = Unsafe.Add(ref GetDefaultAdvanceWidthTable(), (IntPtr) '.');
            }
            else
            {
                _fontLineSpacing = fontFamily.LineSpacing;

                _toGlyphMap = _glyphTypeface.CharacterToGlyphMap;
                _advanceWidthsDict = _glyphTypeface.AdvanceWidths;

                _glyphTypeface.CharacterToGlyphMap.TryGetValue('.', out _dotGlyphIndex);
                _dotAdvanceWidth = _advanceWidthsDict[_dotGlyphIndex] * _fontSize;

                _glyphDataCache = new Dictionary<int, (ushort GlyphIndex, double AdvanceWidth)>();
            }

#if DEBUG
            // グリフデータテーブルを作る
            // 作ったものは、手動でソースコードに組み込む。
            //MakeGlyphDataTable(fontFamily, _glyphTypeface, _fontSize);
#endif
        }

        internal double Draw(
            Visual visual,
            ReadOnlySpan<char> text,
            double x,
            double y,
            Brush brush,
            DrawingContext dc,
            double maxWidth,
            TextAlignment align,
            TextTrimmingMode trimming)
        {
            if (NumberHelper.AreCloseZero(_fontSize))
                return 0;

            if (text.Length == 0)
                return 0;

            maxWidth = Math.Ceiling(maxWidth);

            if (maxWidth <= 0)
                return 0;

            var gr = trimming switch
            {
                TextTrimmingMode.None => MakeGlyphRunNone(visual, text, maxWidth),
                TextTrimmingMode.Standard => MakeGlyphRunStandard(visual, text, maxWidth),
                TextTrimmingMode.Filepath => MakeGlyphRunFilepath(visual, text, maxWidth),
                _ => throw new ArgumentOutOfRangeException(nameof(trimming), trimming, null)
            };

            if (gr == default)
                return 0;

            switch (align)
            {
                case TextAlignment.Left:
                    break;

                case TextAlignment.Right:
                    x += maxWidth - gr.Width;
                    break;

                case TextAlignment.Center:
                    x += (maxWidth - gr.Width) / 2;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(align), align, null);
            }

            if (NumberHelper.AreCloseZero(x) && NumberHelper.AreCloseZero(y))
            {
                dc.DrawGlyphRun(brush, gr.GlyphRun);
            }
            else
            {
                var hash = HashCodeMaker.Make(x, y);

                if (_translateCache.TryGetValue(hash, out var t) == false)
                {
                    t = new TranslateTransform(x, y);
                    _translateCache.Add(hash, t);
                }

                dc.PushTransform(t);
                dc.DrawGlyphRun(brush, gr.GlyphRun);
                dc.Pop();
            }

            return gr.Width;
        }

        internal double CalcWidth(ReadOnlySpan<char> text)
        {
            if (NumberHelper.AreCloseZero(_fontSize))
                return 0;

            if (text.Length == 0)
                return 0;

            var textHashCode = HashCodeMaker.Make(text);

            if (_textWidthCache.TryGetValue(textHashCode, out var textWidth))
                return textWidth;

            if (_isDefault)
            {
                ref var defaultAdvanceWidthTableRef = ref GetDefaultAdvanceWidthTable();

                foreach (var c in text)
                    textWidth += Unsafe.Add(ref defaultAdvanceWidthTableRef, (IntPtr) c);
            }
            else
            {
                Debug.Assert(_glyphDataCache != null);
                Debug.Assert(_toGlyphMap != null);
                Debug.Assert(_advanceWidthsDict != null);

                foreach (var c in text)
                {
                    if (_glyphDataCache.TryGetValue(c, out var data) == false)
                    {
                        if (_toGlyphMap.TryGetValue(c, out data.GlyphIndex) == false)
                            _toGlyphMap.TryGetValue(' ', out data.GlyphIndex);

                        data.AdvanceWidth = _advanceWidthsDict[data.GlyphIndex] * _fontSize;

                        _glyphDataCache.Add(c, data);
                    }

                    textWidth += data.AdvanceWidth;
                }
            }

            _textWidthCache.Add(textHashCode, textWidth);

            return textWidth;
        }

        internal double FontHeight => _fontLineSpacing * _fontSize;

        private (GlyphRun GlyphRun, double Width) MakeGlyphRunNone(Visual visual, ReadOnlySpan<char> text, double maxWidth)
        {
            var glyphIndexes = ArrayPool<ushort>.Shared.Rent(text.Length);
            var advanceWidths = ArrayPool<double>.Shared.Rent(text.Length);

            try
            {
                var textWidth = 0.0;
                var isTrimmed = false;
                var newCount = 0;
                {
                    ref var defaultGlyphIndexTableRef = ref GetDefaultGlyphIndexTable();
                    ref var defaultAdvanceWidthTableRef = ref GetDefaultAdvanceWidthTable();

                    for (var i = 0; i != text.Length; ++i)
                    {
                        var targetChar = text[i];

                        var oldTextWidth = textWidth;

                        if (_isDefault)
                        {
                            glyphIndexes[i] = Unsafe.Add(ref defaultGlyphIndexTableRef, (IntPtr) targetChar);
                            advanceWidths[i] = Unsafe.Add(ref defaultAdvanceWidthTableRef, (IntPtr) targetChar);

                            textWidth += advanceWidths[i];
                        }
                        else
                        {
                            Debug.Assert(_glyphDataCache != null);
                            Debug.Assert(_toGlyphMap != null);
                            Debug.Assert(_advanceWidthsDict != null);

                            if (_glyphDataCache.TryGetValue(targetChar, out var data) == false)
                            {
                                if (_toGlyphMap.TryGetValue(targetChar, out data.GlyphIndex) == false)
                                    _toGlyphMap.TryGetValue(' ', out data.GlyphIndex);

                                data.AdvanceWidth = _advanceWidthsDict[data.GlyphIndex] * _fontSize;

                                _glyphDataCache.Add(targetChar, data);
                            }

                            glyphIndexes[i] = data.GlyphIndex;
                            advanceWidths[i] = data.AdvanceWidth;
                            textWidth += data.AdvanceWidth;
                        }

                        if (textWidth > maxWidth)
                        {
                            textWidth = oldTextWidth;
                            newCount = i - 1;
                            isTrimmed = true;
                            break;
                        }
                    }
                }

                if (isTrimmed && newCount <= 0)
                    return default;

                if (NumberHelper.AreCloseZero(textWidth))
                    return default;

                var dpi = visual.PixelsPerDip();

                var textKey = MakeHashCode(text, textWidth, dpi, TextTrimmingMode.None);

                if (_textCache.TryGetValue(textKey, out var gr))
                    return gr;

                var textLength = isTrimmed
                    ? newCount
                    : text.Length;

                var newGlyphIndexes = new ushort[textLength];
                var newAdvanceWidths = new double[textLength];

                Buffer.BlockCopy(glyphIndexes, 0, newGlyphIndexes, 0, textLength * sizeof(short));
                Buffer.BlockCopy(advanceWidths, 0, newAdvanceWidths, 0, textLength * sizeof(double));

                gr =
                    (new GlyphRun(
                        _glyphTypeface,
                        0,
                        false,
                        _fontSize,
                        (float) dpi,
                        newGlyphIndexes,
                        new Point(0, _glyphTypeface.Baseline * _fontSize),
                        newAdvanceWidths,
                        null, null, null, null, null, null), textWidth);

                _textCache.Add(textKey, gr);

                return gr;
            }
            finally
            {
                ArrayPool<ushort>.Shared.Return(glyphIndexes);
                ArrayPool<double>.Shared.Return(advanceWidths);
            }
        }

        private (GlyphRun GlyphRun, double Width) MakeGlyphRunStandard(Visual visual, ReadOnlySpan<char> text, double maxWidth)
        {
            // ※ +3 「...」 が増えることがあるためのバッファ
            var glyphIndexes = ArrayPool<ushort>.Shared.Rent(text.Length + 3);
            var advanceWidths = ArrayPool<double>.Shared.Rent(text.Length + 3);

            try
            {
                var textWidth = 0.0;
                var isTrimmed = false;
                var newCount = 0;
                {
                    ref var defaultGlyphIndexTableRef = ref GetDefaultGlyphIndexTable();
                    ref var defaultAdvanceWidthTableRef = ref GetDefaultAdvanceWidthTable();

                    for (var i = 0; i != text.Length; ++i)
                    {
                        var targetChar = text[i];

                        if (_isDefault)
                        {
                            glyphIndexes[i] = Unsafe.Add(ref defaultGlyphIndexTableRef, (IntPtr) targetChar);
                            advanceWidths[i] = Unsafe.Add(ref defaultAdvanceWidthTableRef, (IntPtr) targetChar);

                            textWidth += advanceWidths[i];
                        }
                        else
                        {
                            Debug.Assert(_glyphDataCache != null);
                            Debug.Assert(_toGlyphMap != null);
                            Debug.Assert(_advanceWidthsDict != null);

                            if (_glyphDataCache.TryGetValue(targetChar, out var data) == false)
                            {
                                if (_toGlyphMap.TryGetValue(targetChar, out data.GlyphIndex) == false)
                                    _toGlyphMap.TryGetValue(' ', out data.GlyphIndex);

                                data.AdvanceWidth = _advanceWidthsDict[data.GlyphIndex] * _fontSize;

                                _glyphDataCache.Add(targetChar, data);
                            }

                            glyphIndexes[i] = data.GlyphIndex;
                            advanceWidths[i] = data.AdvanceWidth;
                            textWidth += data.AdvanceWidth;
                        }

                        if (textWidth > maxWidth)
                        {
                            (textWidth, newCount) = TrimGlyphRun(glyphIndexes, advanceWidths, textWidth, maxWidth, i + 1);

                            isTrimmed = true;
                            break;
                        }
                    }
                }

                if (NumberHelper.AreCloseZero(textWidth))
                    return default;

                var dpi = visual.PixelsPerDip();

                var textKey = MakeHashCode(text, textWidth, dpi, TextTrimmingMode.Standard);

                if (_textCache.TryGetValue(textKey, out var gr))
                    return gr;

                var textLength = isTrimmed
                    ? newCount
                    : text.Length;

                var newGlyphIndexes = new ushort[textLength];
                var newAdvanceWidths = new double[textLength];

                Buffer.BlockCopy(glyphIndexes, 0, newGlyphIndexes, 0, textLength * sizeof(short));
                Buffer.BlockCopy(advanceWidths, 0, newAdvanceWidths, 0, textLength * sizeof(double));

                gr =
                    (new GlyphRun(
                        _glyphTypeface,
                        0,
                        false,
                        _fontSize,
                        (float) dpi,
                        newGlyphIndexes,
                        new Point(0, _glyphTypeface.Baseline * _fontSize),
                        newAdvanceWidths,
                        null, null, null, null, null, null), textWidth);

                _textCache.Add(textKey, gr);

                return gr;
            }
            finally
            {
                ArrayPool<ushort>.Shared.Return(glyphIndexes);
                ArrayPool<double>.Shared.Return(advanceWidths);
            }
        }

        private (double Width, int NewCount) TrimGlyphRun(
            ushort[] glyphIndexes,
            double[] advanceWidths,
            double textWidth,
            double maxWidth,
            int bufferSize)
        {
            Debug.Assert(textWidth > maxWidth);

            // 文字列に ... を加える分を考慮して削る文字数を求める
            var dot3Width = _dotAdvanceWidth * 3.0;

            var removeCount = 1;
            var newTextWidth = textWidth;
            {
                for (var i = bufferSize - 1; i >= 0; --i)
                {
                    newTextWidth -= advanceWidths[i];

                    if (maxWidth - newTextWidth >= dot3Width)
                        break;

                    ++removeCount;
                }
            }

            var newCount = bufferSize - removeCount + 3;
            if (newCount < 3)
                return (0.0, 0);

            // 文字列に ... を追加する
            glyphIndexes[newCount - 1 - 2] = _dotGlyphIndex;
            glyphIndexes[newCount - 1 - 1] = _dotGlyphIndex;
            glyphIndexes[newCount - 1 - 0] = _dotGlyphIndex;
            advanceWidths[newCount - 1 - 2] = _dotAdvanceWidth;
            advanceWidths[newCount - 1 - 1] = _dotAdvanceWidth;
            advanceWidths[newCount - 1 - 0] = _dotAdvanceWidth;

            return (newTextWidth + dot3Width, newCount);
        }

        private (GlyphRun GlyphRun, double Width) MakeGlyphRunFilepath(Visual visual, ReadOnlySpan<char> text, double maxWidth)
        {
            var buffer = ArrayPool<char>.Shared.Rent(text.Length);

            try
            {
                if (CalcWidth(text) > maxWidth)
                    text = TrimmingFilepathText(text, maxWidth, buffer);

                return MakeGlyphRunStandard(visual, text, maxWidth);
            }
            finally
            {
                ArrayPool<char>.Shared.Return(buffer);
            }
        }

        private ReadOnlySpan<char> TrimmingFilepathText(ReadOnlySpan<char> text, double maxWidth, char[] buffer)
        {
            // ref: https://www.codeproject.com/Tips/467054/WPF-PathTrimmingTextBlock

            bool widthOk;
            var filename = Path.GetFileName(text);
            var directory = Path.GetDirectoryName(text);

            var changedWidth = false;

            ReadOnlySpan<char> sepSpan = "...\\";

            var sepWidth = CalcWidth(sepSpan);
            var filepathWidth = CalcWidth(filename);

            do
            {
                var directoryWidth = CalcWidth(directory);
                var pathWidth = directoryWidth + sepWidth + filepathWidth;

                widthOk = pathWidth < maxWidth;

                if (widthOk == false)
                {
                    changedWidth = true;
                    directory = directory.Slice(0, directory.Length - 1);

                    if (directory.Length == 0)
                    {
                        // "...\\" + new string(filename);
                        sepSpan.CopyTo(buffer);
                        filename.CopyTo(buffer.AsSpan(sepSpan.Length));

                        return buffer.AsSpan(0, sepSpan.Length + filename.Length);
                    }
                }
            } while (widthOk == false);

            if (changedWidth)
            {
                // new string(directory) + "...\\" + new string(filename);
                directory.CopyTo(buffer);
                sepSpan.CopyTo(buffer.AsSpan(directory.Length));
                filename.CopyTo(buffer.AsSpan(directory.Length + sepSpan.Length));

                return buffer.AsSpan(0, directory.Length + sepSpan.Length + filename.Length);
            }
            else
                return text;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static long MakeHashCode(
            ReadOnlySpan<char> text,
            double textWidth,
            double dpi,
            TextTrimmingMode textTrimming)
        {
            unchecked
            {
                var hashCode = HashCodeMaker.Make(text);

                hashCode = (hashCode * 397) ^ (long) textTrimming;

                return (hashCode * 397) ^ HashCodeMaker.Make(textWidth, dpi);
            }
        }

        private readonly LruCache<long, (GlyphRun, double)> _textCache = new LruCache<long, (GlyphRun, double)>(10_0000, false);
        private readonly LruCache<long, double> _textWidthCache = new LruCache<long, double>(10_1000, false);

        private readonly LruCache<long, TranslateTransform> _translateCache = new LruCache<long, TranslateTransform>(1000, false);

        private readonly IDictionary<int, ushort>? _toGlyphMap;
        private readonly IDictionary<ushort, double>? _advanceWidthsDict;

        // 最大65536エントリ
        private readonly Dictionary<int, (ushort GlyphIndex, double AdvanceWidth)>? _glyphDataCache;

        private readonly bool _isDefault;
        private readonly GlyphTypeface? _glyphTypeface;
        private readonly ushort _dotGlyphIndex;
        private readonly double _dotAdvanceWidth;
        private readonly double _fontSize;
        private readonly double _fontLineSpacing;

#if DEBUG
        // ReSharper disable once UnusedMember.Local
        private static void MakeGlyphDataTable(FontFamily fontFamily, GlyphTypeface glyphTypeface, double fontSize)
        {
            var toGlyphMap = glyphTypeface.CharacterToGlyphMap;
            var advanceWidthsDict = glyphTypeface.AdvanceWidths;

            toGlyphMap.TryGetValue(' ', out var dummyIndex);

            var glyphIndexArray = new ushort[char.MaxValue + 1];
            var advanceWidthArray = new double[char.MaxValue + 1];

            for (var i = 0; i <= char.MaxValue; ++i)
            {
                if (toGlyphMap.TryGetValue(i, out var glyphIndex) == false)
                    glyphIndex = dummyIndex;

                var advanceWidth = advanceWidthsDict[glyphIndex] * fontSize;

                glyphIndexArray[i] = glyphIndex;
                advanceWidthArray[i] = advanceWidth;
            }

            var glyphIndexByteArray = MemoryMarshal.Cast<ushort, byte>(glyphIndexArray).ToArray();
            var advanceWidthByteArray = MemoryMarshal.Cast<double, byte>(advanceWidthArray).ToArray();

            var sb = new StringBuilder();

            sb.AppendLine("// ReSharper disable All");
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Runtime.CompilerServices;");
            sb.AppendLine("using System.Runtime.InteropServices;");
            sb.AppendLine("namespace Biaui.Internals");
            sb.AppendLine("{");
            sb.AppendLine("internal partial class TextRenderer");
            sb.AppendLine("{");

            sb.AppendLine($"private static readonly double DefaultFontLineSpacing = {fontFamily.LineSpacing};");

            sb.AppendLine("[MethodImpl(MethodImplOptions.AggressiveInlining)]");
            sb.AppendLine("private static ref ushort GetDefaultGlyphIndexTable(){");
            sb.AppendLine("var byteTable = new ReadOnlySpan<byte>(new byte[] {");
            sb.AppendLine(JoinStrings(glyphIndexByteArray));
            sb.AppendLine("});");
            sb.AppendLine("return ref MemoryMarshal.GetReference(MemoryMarshal.Cast<byte, ushort>(byteTable));");
            sb.AppendLine("}");

            sb.AppendLine("[MethodImpl(MethodImplOptions.AggressiveInlining)]");
            sb.AppendLine("private static ref double GetDefaultAdvanceWidthTable(){");
            sb.AppendLine("var byteTable = new ReadOnlySpan<byte>(new byte[] {");
            sb.AppendLine(JoinStrings(advanceWidthByteArray));
            sb.AppendLine("});");
            sb.AppendLine("return ref MemoryMarshal.GetReference(MemoryMarshal.Cast<byte, double>(byteTable));");
            sb.AppendLine("}");

            sb.AppendLine("}");
            sb.AppendLine("}");

            var outputDir = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile), "TextRenderer.table.cs");
            File.WriteAllText(outputDir, sb.ToString());
        }

        private static string JoinStrings<T>(IEnumerable<T> source)
        {
            var sb = new StringBuilder();

            var c = 0;

            foreach (var v in source)
            {
                ++c;

                sb.Append($"0x{v:x2}");

                if (c > 0 && (c % 32 == 0))
                    sb.AppendLine(",");
                else
                    sb.Append(',');
            }

            return sb.ToString();
        }
#endif
    }
}