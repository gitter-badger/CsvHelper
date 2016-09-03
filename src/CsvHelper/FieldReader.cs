using System;
using System.IO;
using System.Text;
using CsvHelper.Configuration;

namespace CsvHelper
{
    public class FieldReader : IDisposable
    {
	    private string field = string.Empty;
	    private int bufferPosition;
	    private int fieldStartPosition;
	    private int fieldEndPosition;
	    private int rawRecordStartPosition;
	    private int charsRead;
	    private bool disposed;

		public long CharPosition { get; protected set; }

		public long BytePosition { get; protected set; }

	    public char[] Buffer { get; }

		public string RawRecord { get; private set; }

	    public TextReader Reader { get; private set; }

	    public CsvConfiguration Configuration { get; }

	    public FieldReader( TextReader reader, CsvConfiguration configuration )
	    {
			Reader = reader;
		    Buffer = new char[configuration.BufferSize];
		    Configuration = configuration;
	    }

	    public virtual int GetChar()
	    {
		    CheckDisposed();

		    if( bufferPosition >= charsRead )
		    {
			    charsRead = Reader.Read( Buffer, 0, Buffer.Length );
			    if( charsRead == 0 )
			    {
				    return -1;
			    }

				field += new string( Buffer, fieldStartPosition, bufferPosition - fieldStartPosition );
				bufferPosition = 0;
			    fieldStartPosition = 0;
		    }

		    var c = Buffer[bufferPosition];
		    bufferPosition++;

		    CharPosition++;

		    return c;
	    }

	    public virtual string GetField()
	    {
		    AppendField();

		    var result = field;
		    field = string.Empty;

		    return result;
	    }

	    public virtual void AppendField()
	    {
			if( Configuration.CountBytes )
			{
				BytePosition += Configuration.Encoding.GetByteCount( Buffer, fieldStartPosition, bufferPosition - fieldStartPosition );
			}

		    RawRecord += new string( Buffer, rawRecordStartPosition, bufferPosition - rawRecordStartPosition );
		    rawRecordStartPosition = bufferPosition;

			var length = fieldEndPosition - fieldStartPosition;
			field += new string( Buffer, fieldStartPosition, length );
			fieldStartPosition = bufferPosition;
		    fieldEndPosition = bufferPosition;
	    }

		public virtual void SetFieldStart( int offset = 0 )
	    {
			fieldStartPosition = bufferPosition + offset;
	    }

	    public virtual void SetFieldEnd( int offset = 0 )
	    {
		    fieldEndPosition = bufferPosition + offset;
	    }

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		/// <filterpriority>2</filterpriority>
		public virtual void Dispose()
		{
			Dispose( true );
			GC.SuppressFinalize( this );
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		/// <param name="disposing">True if the instance needs to be disposed of.</param>
		protected virtual void Dispose( bool disposing )
		{
			if( disposed )
			{
				return;
			}

			if( disposing )
			{
				if( Reader != null )
				{
					Reader.Dispose();
				}
			}

			disposed = true;
			Reader = null;
		}

		/// <summary>
		/// Checks if the instance has been disposed of.
		/// </summary>
		/// <exception cref="ObjectDisposedException" />
		protected virtual void CheckDisposed()
		{
			if( disposed )
			{
				throw new ObjectDisposedException( GetType().ToString() );
			}
		}
	}
}
