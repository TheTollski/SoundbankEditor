﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SoundbankEditor.Core.WwiseObjects
{
	public interface WwiseObject
	{
		public uint ComputeTotalSize();
		public void WriteToBinary(BinaryWriter binaryWriter);
	}
}
