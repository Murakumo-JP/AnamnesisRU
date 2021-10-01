﻿// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using System.Runtime.InteropServices;

	[StructLayout(LayoutKind.Explicit)]
	public struct Bust
	{
		[FieldOffset(0x68)] public Vector Scale;
	}
}