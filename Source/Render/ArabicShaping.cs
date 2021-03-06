﻿/*
*******************************************************************************
*   Copyright (C) 2001-2004, International Business Machines
*   Corporation and others.  All Rights Reserved.
********************************************************************************/
using System;
using FlexCel.Core;

namespace FlexCel.Render
{	
	/// <summary> Shape Arabic text on a character basis.
	/// 
	/// <p>ArabicShaping performs basic operations for "shaping" Arabic text. It is most
	/// useful for use with legacy data formats and legacy display technology
	/// (simple terminals). All operations are performed on Unicode characters.</p>
	/// 
	/// <p>Text-based shaping means that some character code points in the text are
	/// replaced by others depending on the context. It transforms one kind of text
	/// into another. In comparison, modern displays for Arabic text select
	/// appropriate, context-dependent font glyphs for each text element, which means
	/// that they transform text into a glyph vector.</p>
	/// 
	/// <p>Text transformations are necessary when modern display technology is not
	/// available or when text needs to be transformed to or from legacy formats that
	/// use "shaped" characters. Since the Arabic script is cursive, connecting
	/// adjacent letters to each other, computers select images for each letter based
	/// on the surrounding letters. This usually results in four images per Arabic
	/// letter: initial, middle, final, and isolated forms. In Unicode, on the other
	/// hand, letters are normally stored abstract, and a display system is expected
	/// to select the necessary glyphs. (This makes searching and other text
	/// processing easier because the same letter has only one code.) It is possible
	/// to mimic this with text transformations because there are characters in
	/// Unicode that are rendered as letters with a specific shape
	/// (or cursive connectivity). They were included for interoperability with
	/// legacy systems and codepages, and for unsophisticated display systems.</p>
	/// 
	/// <p>A second kind of text transformations is supported for Arabic digits:
	/// For compatibility with legacy codepages that only include European digits,
	/// it is possible to replace one set of digits by another, changing the
	/// character code points. These operations can be performed for either
	/// Arabic-Indic Digits (U+0660...U+0669) or Eastern (Extended) Arabic-Indic
	/// digits (U+06f0...U+06f9).</p>
	/// 
	/// <p>Some replacements may result in more or fewer characters (code points).
	/// By default, this means that the destination buffer may receive text with a
	/// length different from the source length. Some legacy systems rely on the
	/// length of the text to be constant. They expect extra spaces to be added
	/// or consumed either next to the affected character or at the end of the
	/// text.</p>
	/// @stable ICU 2.0
	/// </summary>
	internal sealed class ArabicShaping
	{
		private int options;
		private bool isLogical; // convenience
		
		/// <summary> Convert a range of text in the source array, putting the result 
		/// into a range of text in the destination array, and return the number
		/// of characters written.
		/// 
		/// </summary>
		/// <param name="source">An array containing the input text
		/// </param>
		/// <param name="sourceStart">The start of the range of text to convert
		/// </param>
		/// <param name="sourceLength">The length of the range of text to convert
		/// </param>
		/// <param name="dest">The destination array that will receive the result.
		/// It may be <code>NULL</code> only if  <code>destSize</code> is 0.  
		/// </param>
		/// <param name="destStart">The start of the range of the destination buffer to use.
		/// </param>
		/// <param name="destSize">The size (capacity) of the destination buffer.
		/// If <code>destSize</code> is 0, then no output is produced,
		/// but the necessary buffer size is returned ("preflighting").  This
		/// does not validate the text against the options, for example, 
		/// if letters are being unshaped, and spaces are being consumed
		/// following lamalef, this will not detect a lamalef without a 
		/// corresponding space.  An error will be thrown when the actual
		/// conversion is attempted.
		/// </param>
		/// <returns> The number of chars written to the destination buffer.
		/// If an error occurs, then no output was written, or it may be
		/// incomplete.
		/// @throws FlexCelException if the text cannot be converted according to the options.
		/// @stable ICU 2.0
		/// </returns>
		public int shape(char[] source, int sourceStart, int sourceLength, char[] dest, int destStart, int destSize)
		{
			if (source == null)
			{
				throw new System.ArgumentException("source cannot be null");
			}
			if (sourceStart < 0 || sourceLength < 0 || sourceStart + sourceLength > source.Length)
			{
				throw new System.ArgumentException("bad source start (" + sourceStart + ") or length (" + sourceLength + ") for buffer of length " + source.Length);
			}
			if (dest == null && destSize != 0)
			{
				throw new System.ArgumentException("null dest requires destSize == 0");
			}
			if ((destSize != 0) && (destStart < 0 || destSize < 0 || destStart + destSize > dest.Length))
			{
				throw new System.ArgumentException("bad dest start (" + destStart + ") or size (" + destSize + ") for buffer of length " + dest.Length);
			}
			
			return internalShape(source, sourceStart, sourceLength, dest, destStart, destSize);
		}
		
		/// <summary> Convert a range of text in place.  This may only be used if the Length option
		/// does not grow or shrink the text.
		/// 
		/// </summary>
		/// <param name="source">An array containing the input text
		/// </param>
		/// <param name="start">The start of the range of text to convert
		/// </param>
		/// <param name="length">The length of the range of text to convert
		/// @throws FlexCelException if the text cannot be converted according to the options.
		/// @stable ICU 2.0
		/// </param>
		public void  shape(char[] source, int start, int length)
		{
			if ((options & LENGTH_MASK) == LENGTH_GROW_SHRINK)
			{
				throw new FlexCelException("Cannot shape in place with length option grow/shrink.");
			}
			shape(source, start, length, source, start, length);
		}
		
		/// <summary> Convert a string, returning the new string.
		/// 
		/// </summary>
		/// <param name="text">the string to convert
		/// </param>
		/// <returns> the converted string
		/// @throws FlexCelException if the string cannot be converted according to the options.
		/// @stable ICU 2.0
		/// </returns>
		public System.String shape(System.String text)
		{
			char[] src = text.ToCharArray();
			char[] dest = src;
			if (((options & LENGTH_MASK) == LENGTH_GROW_SHRINK) && ((options & LETTERS_MASK) == LETTERS_UNSHAPE))
			{
				
				dest = new char[src.Length * 2]; // max
			}
			int len = shape(src, 0, src.Length, dest, 0, dest.Length);
			
			return new System.String(dest, 0, len);
		}
		
		/// <summary> Construct ArabicShaping using the options flags.
		/// The flags are as follows:<br/>
		/// 'LENGTH' flags control whether the text can change size, and if not,
		/// how to maintain the size of the text when LamAlef ligatures are 
		/// formed or broken.<br/>
		/// 'TEXT_DIRECTION' flags control whether the text is read and written
		/// in visual order or in logical order.<br/>
		/// 'LETTERS_SHAPE' flags control whether conversion is to or from
		/// presentation forms.<br/>
		/// 'DIGITS' flags control whether digits are shaped, and whether from
		/// European to Arabic-Indic or vice-versa.<br/>
		/// 'DIGIT_TYPE' flags control whether standard or extended Arabic-Indic
		/// digits are used when performing digit conversion.
		/// @stable ICU 2.0
		/// </summary>
		public ArabicShaping(int options)
		{
			this.options = options;
			if ((options & DIGITS_MASK) > 0x80)
			{
				throw new System.ArgumentException("bad DIGITS options");
			}
			isLogical = (options & TEXT_DIRECTION_MASK) == TEXT_DIRECTION_LOGICAL;
		}
		
		/// <summary> Memory option: allow the result to have a different length than the source.
		/// @stable ICU 2.0
		/// </summary>
		public const int LENGTH_GROW_SHRINK = 0;
		
		/// <summary> Memory option: the result must have the same length as the source.
		/// If more room is necessary, then try to consume spaces next to modified characters.
		/// @stable ICU 2.0
		/// </summary>
		public const int LENGTH_FIXED_SPACES_NEAR = 1;
		
		/// <summary> Memory option: the result must have the same length as the source.
		/// If more room is necessary, then try to consume spaces at the end of the text.
		/// @stable ICU 2.0
		/// </summary>
		public const int LENGTH_FIXED_SPACES_AT_END = 2;
		
		/// <summary> Memory option: the result must have the same length as the source.
		/// If more room is necessary, then try to consume spaces at the beginning of the text.
		/// @stable ICU 2.0
		/// </summary>
		public const int LENGTH_FIXED_SPACES_AT_BEGINNING = 3;
		
		/// <summary> Bit mask for memory options. 
		/// @stable ICU 2.0
		/// </summary>
		public const int LENGTH_MASK = 3;
		
		
		/// <summary> Direction indicator: the source is in logical (keyboard) order. 
		/// @stable ICU 2.0
		/// </summary>
		public const int TEXT_DIRECTION_LOGICAL = 0;
		
		/// <summary> Direction indicator: the source is in visual (display) order, that is,
		/// the leftmost displayed character is stored first.
		/// @stable ICU 2.0
		/// </summary>
		public const int TEXT_DIRECTION_VISUAL_LTR = 4;
		
		/// <summary> Bit mask for direction indicators. 
		/// @stable ICU 2.0
		/// </summary>
		public const int TEXT_DIRECTION_MASK = 4;
		
		
		/// <summary> Letter shaping option: do not perform letter shaping. 
		/// @stable ICU 2.0
		/// </summary>
		public const int LETTERS_NOOP = 0;
		
		/// <summary> Letter shaping option: replace normative letter characters in the U+0600 (Arabic) block,
		/// by shaped ones in the U+FE70 (Presentation Forms B) block. Performs Lam-Alef ligature
		/// substitution.
		/// @stable ICU 2.0
		/// </summary>
		public const int LETTERS_SHAPE = 8;
		
		/// <summary> Letter shaping option: replace shaped letter characters in the U+FE70 (Presentation Forms B) block
		/// by normative ones in the U+0600 (Arabic) block.  Converts Lam-Alef ligatures to pairs of Lam and
		/// Alef characters, consuming spaces if required.
		/// @stable ICU 2.0
		/// </summary>
		public const int LETTERS_UNSHAPE = 0x10;
		
		/// <summary> Letter shaping option: replace normative letter characters in the U+0600 (Arabic) block,
		/// except for the TASHKEEL characters at U+064B...U+0652, by shaped ones in the U+Fe70
		/// (Presentation Forms B) block.  The TASHKEEL characters will always be converted to
		/// the isolated forms rather than to their correct shape.
		/// @stable ICU 2.0
		/// </summary>
		public const int LETTERS_SHAPE_TASHKEEL_ISOLATED = 0x18;
		
		/// <summary> Bit mask for letter shaping options. 
		/// @stable ICU 2.0
		/// </summary>
		public const int LETTERS_MASK = 0x18;
		
		
		/// <summary> Digit shaping option: do not perform digit shaping. 
		/// @stable ICU 2.0
		/// </summary>
		public const int DIGITS_NOOP = 0;
		
		/// <summary> Digit shaping option: Replace European digits (U+0030...U+0039) by Arabic-Indic digits.
		/// @stable ICU 2.0
		/// </summary>
		public const int DIGITS_EN2AN = 0x20;
		
		/// <summary> Digit shaping option: Replace Arabic-Indic digits by European digits (U+0030...U+0039).
		/// @stable ICU 2.0
		/// </summary>
		public const int DIGITS_AN2EN = 0x40;
		
		/// <summary> Digit shaping option:
		/// Replace European digits (U+0030...U+0039) by Arabic-Indic digits
		/// if the most recent strongly directional character
		/// is an Arabic letter (its Bidi direction value is RIGHT_TO_LEFT_ARABIC). 
		/// The initial state at the start of the text is assumed to be not an Arabic,
		/// letter, so European digits at the start of the text will not change.
		/// Compare to DIGITS_ALEN2AN_INIT_AL.
		/// @stable ICU 2.0
		/// </summary>
		public const int DIGITS_EN2AN_INIT_LR = 0x60;
		
		/// <summary> Digit shaping option:
		/// Replace European digits (U+0030...U+0039) by Arabic-Indic digits
		/// if the most recent strongly directional character
		/// is an Arabic letter (its Bidi direction value is RIGHT_TO_LEFT_ARABIC). 
		/// The initial state at the start of the text is assumed to be an Arabic,
		/// letter, so European digits at the start of the text will change.
		/// Compare to DIGITS_ALEN2AN_INT_LR.
		/// @stable ICU 2.0
		/// </summary>
		public const int DIGITS_EN2AN_INIT_AL = 0x80;
		
		// <summary>Not a valid option value. </summary>
		//private const int DIGITS_RESERVED = 0xa0;
		
		/// <summary> Bit mask for digit shaping options. 
		/// @stable ICU 2.0
		/// </summary>
		public const int DIGITS_MASK = 0xe0;
		
		/// <summary> Digit type option: Use Arabic-Indic digits (U+0660...U+0669). 
		/// @stable ICU 2.0
		/// </summary>
		public const int DIGIT_TYPE_AN = 0;
		
		/// <summary> Digit type option: Use Eastern (Extended) Arabic-Indic digits (U+06f0...U+06f9). 
		/// @stable ICU 2.0
		/// </summary>
		public const int DIGIT_TYPE_AN_EXTENDED = 0x100;
		
		/// <summary> Bit mask for digit type options. 
		/// @stable ICU 2.0
		/// </summary>
		public const int DIGIT_TYPE_MASK = 0x0100; // 0x3f00?
		
		/// <summary> @stable ICU 2.0</summary>
		public  override bool Equals(System.Object rhs)
		{
			return rhs != null && rhs.GetType() == typeof(ArabicShaping) && options == ((ArabicShaping) rhs).options;
		}
		
		/// <summary> @stable ICU 2.0</summary>
		///CLOVER:OFF
		public override int GetHashCode()
		{
			return options;
		}
		
		/// <summary> @stable ICU 2.0</summary>
		public override System.String ToString()
		{
			System.Text.StringBuilder buf = new System.Text.StringBuilder(base.ToString());
			buf.Append('[');
			switch (options & LENGTH_MASK)
			{
				
				case LENGTH_GROW_SHRINK:  buf.Append("grow/shrink"); break;
				
				case LENGTH_FIXED_SPACES_NEAR:  buf.Append("spaces near"); break;
				
				case LENGTH_FIXED_SPACES_AT_END:  buf.Append("spaces at end"); break;
				
				case LENGTH_FIXED_SPACES_AT_BEGINNING:  buf.Append("spaces at beginning"); break;
				}
			switch (options & TEXT_DIRECTION_MASK)
			{
				
				case TEXT_DIRECTION_LOGICAL:  buf.Append(", logical"); break;
				
				case TEXT_DIRECTION_VISUAL_LTR:  buf.Append(", visual"); break;
				}
			switch (options & LETTERS_MASK)
			{
				
				case LETTERS_NOOP:  buf.Append(", no letter shaping"); break;
				
				case LETTERS_SHAPE:  buf.Append(", shape letters"); break;
				
				case LETTERS_SHAPE_TASHKEEL_ISOLATED:  buf.Append(", shape letters tashkeel isolated"); break;
				
				case LETTERS_UNSHAPE:  buf.Append(", unshape letters"); break;
				}
			switch (options & DIGITS_MASK)
			{
				
				case DIGITS_NOOP:  buf.Append(", no digit shaping"); break;
				
				case DIGITS_EN2AN:  buf.Append(", shape digits to AN"); break;
				
				case DIGITS_AN2EN:  buf.Append(", shape digits to EN"); break;
				
				case DIGITS_EN2AN_INIT_LR:  buf.Append(", shape digits to AN contextually: default EN"); break;
				
				case DIGITS_EN2AN_INIT_AL:  buf.Append(", shape digits to AN contextually: default AL"); break;
				}
			switch (options & DIGIT_TYPE_MASK)
			{
				
				case DIGIT_TYPE_AN:  buf.Append(", standard Arabic-Indic digits"); break;
				
				case DIGIT_TYPE_AN_EXTENDED:  buf.Append(", extended Arabic-Indic digits"); break;
				}
			buf.Append("]");
			
			return buf.ToString();
		}
		///CLOVER:ON
		
		//
		// ported api
		//
		
		private const int IRRELEVANT = 4;
		private const int LAMTYPE = 16;
		private const int ALEFTYPE = 32;
		
		private const int LINKR = 1;
		private const int LINKL = 2;
		private const int LINK_MASK = 3;
		
		private static readonly int[] irrelevantPos = new int[]{0x0, 0x2, 0x4, 0x6, 0x8, 0xA, 0xC, 0xE};
		
		//private static readonly char[] convertLamAlef = new char[]{'\u0622', '\u0622', '\u0623', '\u0623', '\u0625', '\u0625', '\u0627', '\u0627'};
		
		private static readonly char[] convertNormalizedLamAlef = new char[]{'\u0622', '\u0623', '\u0625', '\u0627'};
		
		private static readonly int[] araLink = new int[]{1 + 32 + 256 * 0x11, 1 + 32 + 256 * 0x13, 1 + 256 * 0x15, 1 + 32 + 256 * 0x17, 1 + 2 + 256 * 0x19, 1 + 32 + 256 * 0x1D, 1 + 2 + 256 * 0x1F, 1 + 256 * 0x23, 1 + 2 + 256 * 0x25, 1 + 2 + 256 * 0x29, 1 + 2 + 256 * 0x2D, 1 + 2 + 256 * 0x31, 1 + 2 + 256 * 0x35, 1 + 256 * 0x39, 1 + 256 * 0x3B, 1 + 256 * 0x3D, 1 + 256 * 0x3F, 1 + 2 + 256 * 0x41, 1 + 2 + 256 * 0x45, 1 + 2 + 256 * 0x49, 1 + 2 + 256 * 0x4D, 1 + 2 + 256 * 0x51, 1 + 2 + 256 * 0x55, 1 + 2 + 256 * 0x59, 1 + 2 + 256 * 0x5D, 0, 0, 0, 0, 0, 1 + 2, 1 + 2 + 256 * 0x61, 1 + 2 + 256 * 0x65, 1 + 2 + 256 * 0x69, 1 + 2 + 16 + 256 * 0x6D, 1 + 2 + 256 * 0x71, 1 + 2 + 256 * 0x75, 1 + 2 + 256 * 0x79, 1 + 256 * 0x7D, 1 + 256 * 0x7F, 1 + 2 + 256 * 0x81, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 0, 0, 0, 0, 0, 0, 1 + 256 * 0x85, 1 + 256 * 0x87, 1 + 256 * 0x89, 1 + 256 * 0x8B, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4, 0, 1 + 32, 1 + 32, 0, 1 + 32, 1, 1, 1 + 2, 1 + 2, 1 + 2, 1 + 2, 1 + 2, 1 + 2, 1 + 2, 1 + 2, 1 + 2, 1 + 2, 1 + 2, 1 + 2, 1 + 2, 1 + 2, 1 + 2, 1 + 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 + 2, 1 + 2, 1 + 2, 1 + 2, 1 + 2, 1 + 2, 1 + 2, 1 + 2, 1 + 2, 1 + 2, 1 + 2, 1 + 2, 1 + 2, 1 + 2, 1 + 2, 1 + 2, 1 + 2, 1 + 2, 1 + 2, 1 + 2, 1 + 2, 1 + 2, 1 + 2, 1 + 2, 1 + 2, 1 + 2, 1 + 2, 1 + 2, 1 + 2, 1 + 2, 1 + 2, 1 + 2, 1 + 2, 1 + 2, 1 + 2, 1 + 2, 1 + 2, 1 + 2, 1, 1 + 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 + 2, 1, 1 + 2, 1 + 2, 1 + 2, 1 + 2, 1, 1};
		
		private static readonly int[] presLink = new int[]{1 + 2, 1 + 2, 1 + 2, 0, 1 + 2, 0, 1 + 2, 1 + 2, 1 + 2, 1 + 2, 1 + 2, 1 + 2, 1 + 2, 1 + 2, 1 + 2, 1 + 2, 0, 0 + 32, 1 + 32, 0 + 32, 1 + 32, 0, 1, 0 + 32, 1 + 32, 0, 2, 1 + 2, 1, 0 + 32, 1 + 32, 0, 2, 1 + 2, 1, 0, 1, 0, 2, 1 + 2, 1, 0, 2, 1 + 2, 1, 0, 2, 1 + 2, 1, 0, 2, 1 + 2, 1, 0, 2, 1 + 2, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 2, 1 + 2, 1, 0, 2, 1 + 2, 1, 0, 2, 1 + 2, 1, 0, 2, 1 + 2, 1, 0, 2, 1 + 2, 1, 0, 2, 1 + 2, 1, 0, 2, 1 + 2, 1, 0, 2, 1 + 2, 1, 0, 2, 1 + 2, 1, 0, 2, 1 + 2, 1, 0, 2, 1 + 2, 1, 0 + 16, 2 + 16, 1 + 2 + 16, 1 + 16, 0, 2, 1 + 2, 1, 0, 2, 1 + 2, 1, 0, 2, 1 + 2, 1, 0, 1, 0, 1, 0, 2, 1 + 2, 1, 0, 1, 0, 1, 0, 1, 0, 1};
		
		private static int[] convertFEto06 = new int[]{0x64B, 0x64B, 0x64C, 0x64C, 0x64D, 0x64D, 0x64E, 0x64E, 0x64F, 0x64F, 0x650, 0x650, 0x651, 0x651, 0x652, 0x652, 0x621, 0x622, 0x622, 0x623, 0x623, 0x624, 0x624, 0x625, 0x625, 0x626, 0x626, 0x626, 0x626, 0x627, 0x627, 0x628, 0x628, 0x628, 0x628, 0x629, 0x629, 0x62A, 0x62A, 0x62A, 0x62A, 0x62B, 0x62B, 0x62B, 0x62B, 0x62C, 0x62C, 0x62C, 0x62C, 0x62D, 0x62D, 0x62D, 0x62D, 0x62E, 0x62E, 0x62E, 0x62E, 0x62F, 0x62F, 0x630, 0x630, 0x631, 0x631, 0x632, 0x632, 0x633, 0x633, 0x633, 0x633, 0x634, 0x634, 0x634, 0x634, 0x635, 0x635, 0x635, 0x635, 0x636, 0x636, 0x636, 0x636, 0x637, 0x637, 0x637, 0x637, 0x638, 0x638, 0x638, 0x638, 0x639, 0x639, 0x639, 0x639, 0x63A, 0x63A, 0x63A, 0x63A, 0x641, 0x641, 0x641, 0x641, 0x642, 0x642, 0x642, 0x642, 0x643, 0x643, 0x643, 0x643, 0x644, 0x644, 0x644, 0x644, 0x645, 0x645, 0x645, 0x645, 0x646, 0x646, 0x646, 0x646, 0x647, 0x647, 0x647, 0x647, 0x648, 0x648, 0x649, 0x649, 0x64A, 0x64A, 0x64A, 0x64A, 0x65C, 0x65C, 0x65D, 0x65D, 0x65E, 0x65E, 0x65F, 0x65F};
		
		private static readonly int[][][] shapeTable = {new int[][]{new int[]{0, 0, 0, 0}, new int[]{0, 0, 0, 0}, new int[]{0, 1, 0, 3}, new int[]{0, 1, 0, 1}}, new int[][]{new int[]{0, 0, 2, 2}, new int[]{0, 0, 1, 2}, new int[]{0, 1, 1, 2}, new int[]{0, 1, 1, 3}}, new int[][]{new int[]{0, 0, 0, 0}, new int[]{0, 0, 0, 0}, new int[]{0, 1, 0, 3}, new int[]{0, 1, 0, 3}}, new int[][]{new int[]{0, 0, 1, 2}, new int[]{0, 0, 1, 2}, new int[]{0, 1, 1, 2}, new int[]{0, 1, 1, 3}}};
		
		/*
		* This function shapes European digits to Arabic-Indic digits
		* in-place, writing over the input characters.  Data is in visual
		* order.
		*/
		private static void  shapeToArabicDigitsWithContext(char[] dest, int start, int length, char digitBase, bool lastStrongWasAL)
		{
			
			digitBase -= '0'; // move common adjustment out of loop
			
			for (int i = start + length; --i >= start; )
			{
				char ch = dest[i];
				switch (BidiReference.Direction(ch))
				{
					
					case BidiReference.L: 
					case BidiReference.R: 
						lastStrongWasAL = false;
						break;
					
					case BidiReference.AL: 
						lastStrongWasAL = true;
						break;
					
					case BidiReference.EN: 
						if (lastStrongWasAL && ch <= '\u0039')
						{
							dest[i] = (char) (ch + digitBase);
						}
						break;
					
					default: 
						break;
					
				}
			}
		}
		
		/*
		* Name    : invertBuffer
		* Function: This function inverts the buffer, it's used
		*           in case the user specifies the buffer to be
		*           TEXT_DIRECTION_LOGICAL
		*/
		private static void  invertBuffer(char[] buffer, int start, int length)
		{
			
			for (int i = start, j = start + length - 1; i < j; i++, --j)
			{
				char temp = buffer[i];
				buffer[i] = buffer[j];
				buffer[j] = temp;
			}
		}
		
		/*
		* Name    : changeLamAlef
		* Function: Converts the Alef characters into an equivalent
		*           LamAlef location in the 0x06xx Range, this is an
		*           intermediate stage in the operation of the program
		*           later it'll be converted into the 0xFExx LamAlefs 
		*           in the shaping function.
		*/
		private static char changeLamAlef(char ch)
		{
			switch (ch)
			{
				
				case '\u0622':  return '\u065C';
				
				case '\u0623':  return '\u065D';
				
				case '\u0625':  return '\u065E';
				
				case '\u0627':  return '\u065F';
				
				default:  return '\u0000'; // not a lamalef
				
			}
		}
		
		/*
		* Name    : specialChar
		* Function: Special Arabic characters need special handling in the shapeUnicode
		*           function, this function returns 1 or 2 for these special characters
		*/
		private static int specialChar(char ch)
		{
			if ((ch > '\u0621' && ch < '\u0626') || (ch == '\u0627') || (ch > '\u062E' && ch < '\u0633') || (ch > '\u0647' && ch < '\u064A') || (ch == '\u0629'))
			{
				return 1;
			}
			else if (ch >= '\u064B' && ch <= '\u0652')
			{
				return 2;
			}
			else if (ch >= 0x0653 && ch <= 0x0655 || ch == 0x0670 || ch >= 0xFE70 && ch <= 0xFE7F)
			{
				return 3;
			}
			else
			{
				return 0;
			}
		}
		
		/*
		* Name    : getLink
		* Function: Resolves the link between the characters as 
		*           Arabic characters have four forms :
		*           Isolated, Initial, Middle and Final Form
		*/
		private static int getLink(char ch)
		{
			if (ch >= '\u0622' && ch <= '\u06D3')
			{
				return araLink[ch - '\u0622'];
			}
			else if (ch == '\u200D')
			{
				return 3;
			}
			else if (ch >= '\u206D' && ch <= '\u206F')
			{
				return 4;
			}
			else if (ch >= '\uFE70' && ch <= '\uFEFC')
			{
				return presLink[ch - '\uFE70'];
			}
			else
			{
				return 0;
			}
		}
		
		/*
		* Name    : countSpaces
		* Function: Counts the number of spaces
		*           at each end of the logical buffer
		*/
		private static int countSpacesLeft(char[] dest, int start, int count)
		{
			for (int i = start, e = start + count; i < e; ++i)
			{
				if (dest[i] != '\u0020')
				{
					return i - start;
				}
			}
			return count;
		}
		
		private static int countSpacesRight(char[] dest, int start, int count)
		{
			
			for (int i = start + count; --i >= start; )
			{
				if (dest[i] != '\u0020')
				{
					return start + count - 1 - i;
				}
			}
			return count;
		}
		
		/*
		* Name    : isTashkeelChar
		* Function: Returns 1 for Tashkeel characters else return 0
		*/
		private static bool isTashkeelChar(char ch)
		{
			return ch >= '\u064B' && ch <= '\u0652';
		}
		
		/*
		* Name    : isAlefChar
		* Function: Returns 1 for Alef characters else return 0
		*/
		private static bool isAlefChar(char ch)
		{
			return ch == '\u0622' || ch == '\u0623' || ch == '\u0625' || ch == '\u0627';
		}
		
		/*
		* Name    : isLamAlefChar
		* Function: Returns 1 for LamAlef characters else return 0
		*/
		private static bool isLamAlefChar(char ch)
		{
			return ch >= '\uFEF5' && ch <= '\uFEFC';
		}
		
		private static bool isNormalizedLamAlefChar(char ch)
		{
			return ch >= '\u065C' && ch <= '\u065F';
		}
		
		/*
		* Name    : calculateSize
		* Function: This function calculates the destSize to be used in preflighting
		*           when the destSize is equal to 0
		*/
		private int calculateSize(char[] source, int sourceStart, int sourceLength)
		{
			
			int destSize = sourceLength;
			
			switch (options & LETTERS_MASK)
			{
				
				case LETTERS_SHAPE: 
				case LETTERS_SHAPE_TASHKEEL_ISOLATED: 
					if (isLogical)
					{
						for (int i = sourceStart, e = sourceStart + sourceLength - 1; i < e; ++i)
						{
							if (source[i] == '\u0644' && isAlefChar(source[i + 1]))
							{
								--destSize;
							}
						}
					}
					else
					{
						// visual
						for (int i = sourceStart + 1, e = sourceStart + sourceLength; i < e; ++i)
						{
							if (source[i] == '\u0644' && isAlefChar(source[i - 1]))
							{
								--destSize;
							}
						}
					}
					break;
				
				
				case LETTERS_UNSHAPE: 
					for (int i = sourceStart, e = sourceStart + sourceLength; i < e; ++i)
					{
						if (isLamAlefChar(source[i]))
						{
							destSize++;
						}
					}
					break;
				
				
				default: 
					break;
				
			}
			
			return destSize;
		}
		
		/*
		* Name    : removeLamAlefSpaces
		* Function: The shapeUnicode function converts Lam + Alef into LamAlef + space,
		*           this function removes the spaces behind the LamAlefs according to
		*           the options the user specifies, the spaces are removed to the end
		*           of the buffer, or shrink the buffer and remove spaces for good
		*           or leave the buffer as it is LamAlef + space.
		*/
		private int removeLamAlefSpaces(char[] dest, int start, int length)
		{
			
			int lenOptions = options & LENGTH_MASK;
			if (!isLogical)
			{
				switch (lenOptions)
				{
					
					case LENGTH_FIXED_SPACES_AT_BEGINNING:  lenOptions = LENGTH_FIXED_SPACES_AT_END; break;
					
					case LENGTH_FIXED_SPACES_AT_END:  lenOptions = LENGTH_FIXED_SPACES_AT_BEGINNING; break;
					
					default:  break;
					
				}
			}
			
			if (lenOptions == LENGTH_FIXED_SPACES_NEAR)
			{
				for (int i = start, e = i + length; i < e; ++i)
				{
					if (dest[i] == '\uffff')
					{
						dest[i] = '\u0020';
					}
				}
			}
			else
			{
				int e = start + length;
				int w = e;
				int r = e;
				while (--r >= start)
				{
					char ch = dest[r];
					if (ch != '\uffff')
					{
						--w;
						if (w != r)
						{
							dest[w] = ch;
						}
					}
				}
				
				if (lenOptions == LENGTH_FIXED_SPACES_AT_END)
				{
					while (w > start)
					{
						dest[--w] = '\u0020';
					}
				}
				else
				{
					if (w > start)
					{
						// shift, assume small buffer size so don't use arraycopy
						r = w;
						w = start;
						while (r < e)
						{
							dest[w++] = dest[r++];
						}
					}
					else
					{
						w = e;
					}
					if (lenOptions == LENGTH_GROW_SHRINK)
					{
						length = w - start;
					}
					else
					{
						// spaces at beginning
						while (w < e)
						{
							dest[w++] = '\u0020';
						}
					}
				}
			}
			return length;
		}
		
		/*
		* Name    : expandLamAlef
		* Function: LamAlef needs special handling as the LamAlef is
		*           one character while expanding it will give two
		*           characters Lam + Alef, so we need to expand the LamAlef
		*           in near or far spaces according to the options the user
		*           specifies or increase the buffer size.
		*           Dest has enough room for the expansion if we are growing.
		*           lamalef are normalized to the 'special characters'
		*/
		private int expandLamAlef(char[] dest, int start, int length, int lacount)
		{
			
			int lenOptions = options & LENGTH_MASK;
			if (!isLogical)
			{
				switch (lenOptions)
				{
					
					case LENGTH_FIXED_SPACES_AT_BEGINNING:  lenOptions = LENGTH_FIXED_SPACES_AT_END; break;
					
					case LENGTH_FIXED_SPACES_AT_END:  lenOptions = LENGTH_FIXED_SPACES_AT_BEGINNING; break;
					
					default:  break;
					
				}
			}
			
			switch (lenOptions)
			{
				
				case LENGTH_GROW_SHRINK: 
					{
						for (int r = start + length, w = r + lacount; --r >= start; )
						{
							char ch = dest[r];
							if (isNormalizedLamAlefChar(ch))
							{
								dest[--w] = '\u0644';
								dest[--w] = convertNormalizedLamAlef[ch - '\u065C'];
							}
							else
							{
								dest[--w] = ch;
							}
						}
					}
					length += lacount;
					break;
				
				
				case LENGTH_FIXED_SPACES_NEAR: 
					{
						if (isNormalizedLamAlefChar(dest[start]))
						{
							throw new FlexCelException("no space for lamalef");
						}
						for (int i = start + length; --i > start; )
						{
							// don't check start, already checked
							char ch = dest[i];
							if (isNormalizedLamAlefChar(ch))
							{
								if (dest[i - 1] == '\u0020')
								{
									dest[i] = '\u0644';
									dest[--i] = convertNormalizedLamAlef[ch - '\u065C'];
								}
								else
								{
									throw new FlexCelException("no space for lamalef");
								}
							}
						}
					}
					break;
				
				
				case LENGTH_FIXED_SPACES_AT_END: 
					{
						if (lacount > countSpacesLeft(dest, start, length))
						{
							throw new FlexCelException("no space for lamalef");
						}
						for (int r = start + lacount, w = start, e = start + length; r < e; ++r)
						{
							char ch = dest[r];
							if (isNormalizedLamAlefChar(ch))
							{
								dest[w++] = convertNormalizedLamAlef[ch - '\u065C'];
								dest[w++] = '\u0644';
							}
							else
							{
								dest[w++] = ch;
							}
						}
					}
					break;
				
				
				case LENGTH_FIXED_SPACES_AT_BEGINNING: 
					{
						if (lacount > countSpacesRight(dest, start, length))
						{
							throw new FlexCelException("no space for lamalef");
						}
						for (int r = start + length - lacount, w = start + length; --r >= start; )
						{
							char ch = dest[r];
							if (isNormalizedLamAlefChar(ch))
							{
								dest[--w] = '\u0644';
								dest[--w] = convertNormalizedLamAlef[ch - '\u065C'];
							}
							else
							{
								dest[--w] = ch;
							}
						}
					}
					break;
				}
			
			return length;
		}
		
		/* Convert the input buffer from FExx Range into 06xx Range
		* to put all characters into the 06xx range
		* even the lamalef is converted to the special region in
		* the 06xx range.  Return the number of lamalef chars found.
		*/
		private static int normalize(char[] dest, int start, int length)
		{
			int lacount = 0;
			for (int i = start, e = i + length; i < e; ++i)
			{
				char ch = dest[i];
				if (ch >= '\uFE70' && ch <= '\uFEFC')
				{
					if (isLamAlefChar(ch))
					{
						++lacount;
					}
					dest[i] = (char) convertFEto06[ch - '\uFE70'];
				}
			}
			return lacount;
		}
		
		/*
		* Name    : shapeUnicode
		* Function: Converts an Arabic Unicode buffer in 06xx Range into a shaped
		*           arabic Unicode buffer in FExx Range
		*/
		private int shapeUnicode(char[] dest, int start, int length, int destSize, int tashkeelFlag)
		{
			
			
			normalize(dest, start, length);
			
			// resolve the link between the characters.
			// Arabic characters have four forms: Isolated, Initial, Medial and Final.
			// Tashkeel characters have two, isolated or medial, and sometimes only isolated.
			// tashkeelFlag == 0: shape normally, 1: shape isolated, 2: don't shape
			
			bool lamalef_found = false;
			int i = start + length - 1;
			int currLink = getLink(dest[i]);
			int nextLink = 0;
			int prevLink = 0;
			int lastLink = 0;
			//int prevPos = i;
			int lastPos = i;
			int nx = - 2;
			int nw = 0;
			
			while (i >= 0)
			{
				// If high byte of currLink > 0 then there might be more than one shape
				if ((currLink & '\uFF00') > 0 || isTashkeelChar(dest[i]))
				{
					nw = i - 1;
					nx = - 2;
					while (nx < 0)
					{
						// we need to know about next char
						if (nw == - 1)
						{
							nextLink = 0;
							nx = System.Int32.MaxValue;
						}
						else
						{
							nextLink = getLink(dest[nw]);
							if ((nextLink & IRRELEVANT) == 0)
							{
								nx = nw;
							}
							else
							{
								--nw;
							}
						}
					}
					
					if (((currLink & ALEFTYPE) > 0) && ((lastLink & LAMTYPE) > 0))
					{
						lamalef_found = true;
						char wLamalef = changeLamAlef(dest[i]); // get from 0x065C-0x065f
						if (wLamalef != '\u0000')
						{
							// replace alef by marker, it will be removed later
							dest[i] = '\uffff';
							dest[lastPos] = wLamalef;
							i = lastPos;
						}
						
						lastLink = prevLink;
						currLink = getLink(wLamalef); // requires '\u0000', unfortunately
					}
					
					// get the proper shape according to link ability of neighbors
					// and of character; depends on the order of the shapes
					// (isolated, initial, middle, final) in the compatibility area
					
					int flag = specialChar(dest[i]);
					
					int shape = shapeTable[nextLink & LINK_MASK][lastLink & LINK_MASK][currLink & LINK_MASK];
					
					if (flag == 1)
					{
						shape &= 0x1;
					}
					else if (flag == 2)
					{
						if (tashkeelFlag == 0 && ((lastLink & LINKL) != 0) && ((nextLink & LINKR) != 0) && dest[i] != '\u064C' && dest[i] != '\u064D' && !((nextLink & ALEFTYPE) == ALEFTYPE && (lastLink & LAMTYPE) == LAMTYPE))
						{
							
							shape = 1;
						}
						else
						{
							shape = 0;
						}
					}
					
					if (flag == 2)
					{
						if (tashkeelFlag < 2)
						{
							dest[i] = (char) ('\uFE70' + irrelevantPos[dest[i] - '\u064B'] + shape);
						} // else leave tashkeel alone                    
					}
					else
					{
						dest[i] = (char) ('\uFE70' + (currLink >> 8) + shape);
					}
				}
				
				// move one notch forward
				if ((currLink & IRRELEVANT) == 0)
				{
					prevLink = lastLink;
					lastLink = currLink;
					//prevPos = lastPos;
					lastPos = i;
				}
				
				--i;
				if (i == nx)
				{
					currLink = nextLink;
					nx = - 2;
				}
				else if (i != - 1)
				{
					currLink = getLink(dest[i]);
				}
			}
			
			// If we found a lam/alef pair in the buffer 
			// call removeLamAlefSpaces to remove the spaces that were added
			
			if (lamalef_found)
			{
				destSize = removeLamAlefSpaces(dest, start, length);
			}
			else
			{
				destSize = length;
			}
			
			return destSize;
		}
		
		/*
		* Name    : deShapeUnicode
		* Function: Converts an Arabic Unicode buffer in FExx Range into unshaped
		*           arabic Unicode buffer in 06xx Range
		*/
		private int deShapeUnicode(char[] dest, int start, int length, int destSize)
		{
			
			int lamalef_count = normalize(dest, start, length);
			
			// If there was a lamalef in the buffer call expandLamAlef
			if (lamalef_count != 0)
			{
				// need to adjust dest to fit expanded buffer... !!!
				destSize = expandLamAlef(dest, start, length, lamalef_count);
			}
			else
			{
				destSize = length;
			}
			
			return destSize;
		}
		
		private int internalShape(char[] source, int sourceStart, int sourceLength, char[] dest, int destStart, int destSize)
		{
			
			if (sourceLength == 0)
			{
				return 0;
			}
			
			if (destSize == 0)
			{
				if (((options & LETTERS_MASK) != LETTERS_NOOP) && ((options & LENGTH_MASK) == LENGTH_GROW_SHRINK))
				{
					
					return calculateSize(source, sourceStart, sourceLength);
				}
				else
				{
					return sourceLength; // by definition
				}
			}
			
			// always use temp buffer
			char[] temp = new char[sourceLength * 2]; // all lamalefs requiring expansion
			Array.Copy((System.Array) source, sourceStart, (System.Array) temp, 0, sourceLength);
			
			if (isLogical)
			{
				invertBuffer(temp, 0, sourceLength);
			}
			
			int outputSize = sourceLength;
			
			switch (options & LETTERS_MASK)
			{
				
				case LETTERS_SHAPE_TASHKEEL_ISOLATED: 
					outputSize = shapeUnicode(temp, 0, sourceLength, destSize, 1);
					break;
				
				
				case LETTERS_SHAPE: 
					outputSize = shapeUnicode(temp, 0, sourceLength, destSize, 0);
					break;
				
				
				case LETTERS_UNSHAPE: 
					outputSize = deShapeUnicode(temp, 0, sourceLength, destSize);
					break;
				
				
				default: 
					break;
				
			}
			
			if (outputSize > destSize)
			{
				throw new FlexCelException("not enough room for result data");
			}
			
			if ((options & DIGITS_MASK) != DIGITS_NOOP)
			{
				char digitBase = '\u0030'; // European digits
				switch (options & DIGIT_TYPE_MASK)
				{
					
					case DIGIT_TYPE_AN: 
						digitBase = '\u0660'; // Arabic-Indic digits
						break;
					
					
					case DIGIT_TYPE_AN_EXTENDED: 
						digitBase = '\u06f0'; // Eastern Arabic-Indic digits (Persian and Urdu)
						break;
					
					
					default: 
						break;
					
				}
				
				switch (options & DIGITS_MASK)
				{
					
					case DIGITS_EN2AN: 
						{
							int digitDelta = digitBase - '\u0030';
							for (int i = 0; i < outputSize; ++i)
							{
								char ch = temp[i];
								if (ch <= '\u0039' && ch >= '\u0030')
								{
									temp[i] += (char) (digitDelta);
								}
							}
						}
						break;
					
					
					case DIGITS_AN2EN: 
						{
							char digitTop = (char) (digitBase + 9);
							int digitDelta = '\u0030' - digitBase;
							for (int i = 0; i < outputSize; ++i)
							{
								char ch = temp[i];
								if (ch <= digitTop && ch >= digitBase)
								{
									temp[i] += (char) (digitDelta);
								}
							}
						}
						break;
					
					
					case DIGITS_EN2AN_INIT_LR: 
						shapeToArabicDigitsWithContext(temp, 0, outputSize, digitBase, false);
						break;
					
					
					case DIGITS_EN2AN_INIT_AL: 
						shapeToArabicDigitsWithContext(temp, 0, outputSize, digitBase, true);
						break;
					
					
					default: 
						break;
					
				}
			}
			
			if (isLogical)
			{
				invertBuffer(temp, 0, outputSize);
			}
			
			Array.Copy((System.Array) temp, 0, (System.Array) dest, destStart, outputSize);
			
			return outputSize;
		}
	}
}
