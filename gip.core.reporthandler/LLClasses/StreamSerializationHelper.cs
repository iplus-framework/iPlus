using System;
using System.IO;
using combit.ListLabel17;
using System.Reflection;

namespace gip.core.reporthandler
{
	internal static class StreamSerializationHelper
	{
        internal static string LlGetTempFileName(string prefix, string fileExtension, int options)
        {
            BindingFlags bindingFlags = BindingFlags.Static | BindingFlags.NonPublic;
            MethodInfo minfo = typeof(LlCore).GetMethod("LlGetTempFileName", bindingFlags);
            return (String) minfo.Invoke(null, new object[] { prefix, fileExtension, options });
            //Path.GetTempPath();
        }

		internal static string StreamToFile(Stream stream, string fileNameBase, string extension, bool findFileName)
		{
			if (string.IsNullOrEmpty(fileNameBase))
			{
				throw new ArgumentNullException(fileNameBase);
			}
			string text;
			if (findFileName)
			{
				text = LlGetTempFileName(Path.GetFileNameWithoutExtension(fileNameBase), extension, 0);
			}
			else
			{
				text = fileNameBase;
				text = Path.ChangeExtension(text, extension);
			}
			BinaryReader binaryReader = new BinaryReader(stream);
			byte[] array = new byte[stream.Length];
			binaryReader.Read(array, 0, array.Length);
			FileStream fileStream = null;
			try
			{
				fileStream = new FileStream(text, FileMode.OpenOrCreate, FileAccess.ReadWrite);
				using (BinaryWriter binaryWriter = new BinaryWriter(fileStream))
				{
					fileStream = null;
					binaryWriter.Write(array);
				}
			}
			finally
			{
				if (fileStream != null)
				{
					fileStream.Dispose();
				}
			}
			return text;
		}

		internal static void FileToStream(string fileName, Stream stream)
		{
			Stream stream2 = null;
			try
			{
				stream2 = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
				using (BinaryReader binaryReader = new BinaryReader(stream2))
				{
					byte[] array = new byte[stream2.Length];
					stream2 = null;
					binaryReader.Read(array, 0, array.Length);
					stream.Position = 0L;
					stream.SetLength((long)array.Length);
					BinaryWriter binaryWriter = new BinaryWriter(stream);
					binaryWriter.Write(array);
					stream.Flush();
				}
			}
			catch (FileNotFoundException e)
			{
				stream.SetLength(0L);

                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == datamodel.ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("StreamSerializationHelper", "FileToStream", msg);
            }
			finally
			{
				if (stream2 != null)
				{
					stream2.Dispose();
				}
			}
		}
	}
}
