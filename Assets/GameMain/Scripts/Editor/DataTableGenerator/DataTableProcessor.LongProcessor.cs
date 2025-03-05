//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2020 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System.IO;

namespace Aki.Editor.DataTableTools
{
    public sealed partial class DataTableProcessor
    {
        private sealed class LongProcessor : GenericDataProcessor<long>
        {
            public override bool IsSystem
            {
                get
                {
                    return true;
                }
            }

            public override string LanguageKeyword
            {
                get
                {
                    return "long";
                }
            }

            public override string[] GetTypeStrings()
            {
                return new string[]
                {
                    "long",
                    "int64",
                    "system.int64"
                };
            }

            public override long Parse(string value)
            {
                return long.Parse(value);
            }

            public override void WriteToStream(DataTableProcessor dataTableProcessor, BinaryWriter binaryWriter, string value)
            {
                Write7BitEncodedInt64(binaryWriter, Parse(value));
            }
            private void Write7BitEncodedInt64(BinaryWriter writer, long value)
            {
                ulong v = (ulong)value;
                while (v >= 0x80)
                {
                    writer.Write((byte)(v | 0x80));
                    v >>= 7;
                }
                writer.Write((byte)v);
            }
        }
    }
}


