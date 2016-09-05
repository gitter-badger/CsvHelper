using System;
using System.IO;
using System.Text;
using CsvHelper.Configuration;

namespace CsvHelper
{
    public class FieldReader : IDisposable
    {
	    private char[] buffer;
	    private string field = string.Empty;
	    private int bufferPosition;
	    private int fieldStartPosition;
	    private int fieldEndPosition;
	    private int rawRecordStartPosition;
	    private int rawRecordEndPosition;
	    private int charsRead;
	    private bool disposed;

		public long CharPosition { get; protected set; }

		public long BytePosition { get; protected set; }

		public string RawRecord { get; private set; }

	    public TextReader Reader { get; private set; }

	    public CsvConfiguration Configuration { get; }

		public bool IsFieldBad { get; set; }

	    public FieldReader( TextReader reader, CsvConfiguration configuration )
	    {
			Reader = reader;
		    buffer = new char[configuration.BufferSize];
		    Configuration = configuration;
	    }

	    public virtual int GetChar()
	    {
		    CheckDisposed();

		    if( bufferPosition >= charsRead )
		    {
				if( Configuration.CountBytes )
				{
					BytePosition += Configuration.Encoding.GetByteCount( buffer, rawRecordStartPosition, rawRecordEndPosition - rawRecordStartPosition );
				}

				RawRecord += new string( buffer, rawRecordStartPosition, bufferPosition - rawRecordStartPosition );
				rawRecordStartPosition = 0;

			    if( fieldEndPosition <= fieldStartPosition )
			    {
					// If the end position hasn't been set yet, use the buffer position instead.
				    fieldEndPosition = bufferPosition;
			    }

				field += new string( buffer, fieldStartPosition, fieldEndPosition - fieldStartPosition );
				bufferPosition = 0;
			    rawRecordEndPosition = 0;
				fieldStartPosition = 0;
			    fieldEndPosition = 0;

				charsRead = Reader.Read( buffer, 0, buffer.Length );
			    if( charsRead == 0 )
			    {
				    return -1;
			    }
		    }

		    var c = buffer[bufferPosition];
		    bufferPosition++;
		    rawRecordEndPosition = bufferPosition;

		    CharPosition++;

		    return c;
	    }

	    public virtual string GetField()
	    {
		    AppendField();

			if( IsFieldBad && Configuration.ThrowOnBadData )
			{
				throw new CsvBadDataException( $"Field: '{field}'" );
			}

			if( IsFieldBad )
			{
				Configuration.BadDataCallback?.Invoke( field );
			}

			IsFieldBad = false;

			var result = field;
		    field = string.Empty;

		    return result;
	    }

	    public virtual void AppendField()
	    {
			if( Configuration.CountBytes )
			{
				BytePosition += Configuration.Encoding.GetByteCount( buffer, rawRecordStartPosition, bufferPosition - rawRecordStartPosition );
			}

		    RawRecord += new string( buffer, rawRecordStartPosition, rawRecordEndPosition - rawRecordStartPosition );
		    rawRecordStartPosition = rawRecordEndPosition;

			var length = fieldEndPosition - fieldStartPosition;
			field += new string( buffer, fieldStartPosition, length );
			fieldStartPosition = bufferPosition;
		    fieldEndPosition = 0;
	    }

		public virtual void SetFieldStart( int offset = 0 )
	    {
			var position = bufferPosition + offset;
			if( position >= 0 )
			{
				fieldStartPosition = position;
			}
	    }

	    public virtual void SetFieldEnd( int offset = 0 )
	    {
		    var position = bufferPosition + offset;
		    if( position >= 0 )
		    {
			    fieldEndPosition = position;
		    }
	    }

	    public virtual void SetRawRecordEnd( int offset )
	    {
		    var position = bufferPosition + offset;
		    if( position >= 0 )
		    {
				rawRecordEndPosition = position;
			}
	    }

	    public virtual void ClearRawRecord()
	    {
			RawRecord = string.Empty;
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
