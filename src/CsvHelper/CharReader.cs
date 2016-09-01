using System;
using System.IO;
using System.Text;

namespace CsvHelper
{
    public class CharReader : IDisposable
    {
	    private string field = string.Empty;
	    private readonly char[] buffer;
		private TextReader reader;
		private int bufferPosition;
	    private int fieldStartPosition;
	    private int charsRead;
	    private bool disposed;

		public long CharPosition { get; protected set; }

		public CharReader( TextReader reader, int bufferSize )
	    {
			this.reader = reader;
		    buffer = new char[bufferSize];
	    }

	    public virtual void GetChar( out char c )
	    {
		    CheckDisposed();

		    if( bufferPosition == charsRead )
		    {
			    charsRead = reader.Read( buffer, 0, buffer.Length );
			    if( charsRead == 0 )
			    {
				    c = '\0';
				    return;
			    }

				field += new string( buffer, fieldStartPosition, bufferPosition - fieldStartPosition );
				bufferPosition = 0;
			    fieldStartPosition = 0;
		    }

		    c = buffer[bufferPosition];
		    bufferPosition++;

		    CharPosition++;
	    }

	    public virtual string GetField( int offset = 0 )
	    {
		    AppendField( offset );

		    var result = field;
		    field = string.Empty;

		    return result;
	    }

	    public virtual void AppendField( int offset = 0 )
	    {
			var length = bufferPosition - fieldStartPosition - Math.Abs( offset );
			field += new string( buffer, fieldStartPosition, length );
			fieldStartPosition = bufferPosition;
		}

		public virtual void SetFieldStart()
	    {
		    fieldStartPosition = bufferPosition;
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
				if( reader != null )
				{
					reader.Dispose();
				}
			}

			disposed = true;
			reader = null;
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
