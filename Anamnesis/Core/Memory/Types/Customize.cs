﻿// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using System;
	using System.Runtime.InteropServices;

	[StructLayout(LayoutKind.Sequential)]
	public struct Customize
	{
		public Races Race;
		public Genders Gender;
		public Ages Age;
		public byte Height;
		public Tribes Tribe;
		public byte Head;
		public byte Hair;
		public byte HighlightType;
		public byte Skintone;
		public byte REyeColor;
		public byte HairTone;
		public byte Highlights;
		public FacialFeature FacialFeatures;
		public byte FacialFeatureColor;
		public byte Eyebrows;
		public byte LEyeColor;
		public byte Eyes;
		public byte Nose;
		public byte Jaw;
		public byte Mouth;
		public byte LipsToneFurPattern;
		public byte EarMuscleTailSize;
		public byte TailEarsType;
		public byte Bust;
		public byte FacePaint;
		public byte FacePaintColor;

		public enum Genders : byte
		{
			Masculine = 0,
			Feminine = 1,
		}

		public enum Races : byte
		{
			Hyur = 1,
			Elezen = 2,
			Lalafel = 3,
			Miqote = 4,
			Roegadyn = 5,
			AuRa = 6,
			Hrothgar = 7,
			Viera = 8,
		}

		public enum Tribes : byte
		{
			Midlander = 1,
			Highlander = 2,
			Wildwood = 3,
			Duskwight = 4,
			Plainsfolk = 5,
			Dunesfolk = 6,
			SeekerOfTheSun = 7,
			KeeperOfTheMoon = 8,
			SeaWolf = 9,
			Hellsguard = 10,
			Raen = 11,
			Xaela = 12,
			Helions = 13,
			TheLost = 14,
			Rava = 15,
			Veena = 16,
		}

		public enum Ages : byte
		{
			Normal = 1,
			Old = 3,
			Young = 4,
		}

		[Flags]
		public enum FacialFeature : byte
		{
			None = 0,
			First = 1,
			Second = 2,
			Third = 4,
			Fourth = 8,
			Fifth = 16,
			Sixth = 32,
			Seventh = 64,
			LegacyTattoo = 128,
		}

		public static Customize Default()
		{
			Customize ap = default;

			ap.Race = Races.Hyur;
			ap.Gender = Genders.Feminine;
			ap.Age = Ages.Normal;
			ap.Tribe = Tribes.Midlander;

			return ap;
		}
	}
}