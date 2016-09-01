using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper.Configuration;

namespace CsvHelper
{
    public class CsvParser : ICsvParser
    {
		private readonly RecordBuilder record = new RecordBuilder();
		private CharReader reader;
		private bool disposed;
	    private int currentRow;
	    private int currentRawRow;
	    private char c = '\0';

		public virtual CsvConfiguration Configuration { get; }

	    public virtual int FieldCount { get; protected set; }

	    public virtual long CharPosition => reader.CharPosition;

		public virtual long BytePosition { get; }

		public virtual int Row => currentRow;

		public virtual int RawRow => currentRawRow;

	    public virtual string RawRecord { get; protected set; }

	    public CsvParser( TextReader reader ) : this( reader, new CsvConfiguration() ) { }

	    public CsvParser( TextReader reader, CsvConfiguration configuration )
	    {
		    if( reader == null )
		    {
			    throw new ArgumentNullException( nameof( reader ) );
		    }

		    if( configuration == null )
		    {
			    throw new ArgumentNullException( nameof( configuration ) );
		    }

		    this.reader = new CharReader( reader, configuration.BufferSize );
		    Configuration = configuration;
	    }

		public virtual string[] Read()
		{
			CheckDisposed();

			try
			{
				var row = ReadLine();

				return row;
			}
			catch( Exception ex )
			{
				ExceptionHelper.AddExceptionDataMessage( ex, this, null, null, null, null );
				throw;
			}
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

	    protected virtual string[] ReadLine()
	    {
		    record.Clear();
			RawRecord = string.Empty;
		    currentRow++;
		    currentRawRow++;

			while( true )
			{
				reader.GetChar( out c );

			    if( c == '\0' )
			    {
					// We have reached the end of the file.

					if( record.Length > 0 )
				    {
						// There was no line break at the end of the file.
						// We need to return the last record first.
						record.Add( reader.GetField() );
						return record.ToArray();
					}

					return null;
			    }

			    if( record.Length == 0 && ( ( c == Configuration.Comment && Configuration.AllowComments ) || c == '\r' || c == '\n' ) )
			    {
				    ReadBlankLine();
				    if( !Configuration.IgnoreBlankLines )
				    {
						break;
				    }

				    continue;
			    }

				if( c == Configuration.Quote )
			    {
				    if( ReadQuotedField() )
				    {
						break;
				    }
			    }
			    else
			    {
				    if( ReadField() )
				    {
					    break;
				    }
			    }
			}

			return record.ToArray();
	    }

		/// <summary>
		/// Reads a blank line. This accounts for empty lines
		/// and commented out lines.
		/// </summary>
	    protected virtual void ReadBlankLine()
	    {
			if( Configuration.IgnoreBlankLines )
			{
				currentRow++;
			}

			while( true )
		    {
			    if( c == '\r' || c == '\n' )
			    {
				    ReadLineEnding();
					reader.GetField();
					return;
			    }

				reader.GetChar( out c );
		    }
	    }

		/// <summary>
		/// Reads until a delimiter or line ending is found.
		/// </summary>
		/// <returns>True if the end of the line was found, otherwise false.</returns>
	    protected virtual bool ReadField()
		{
			if( c != Configuration.Delimiter[0] && c != '\r' && c != '\n' )
			{
				reader.GetChar( out c );
			}

			while( true )
			{
				if( c == Configuration.Delimiter[0] )
				{
					// End of field.
					if( ReadDelimiter() )
					{
						record.Add( reader.GetField( Configuration.Delimiter.Length ) );
						return false;
					}
				}
				else if( c == '\r' || c == '\n' )
				{
					// End of line.
					record.Add( reader.GetField( 1 ) );
					if( ReadLineEnding() )
					{
						reader.SetFieldStart();
					}
					return true;
				}
				else if( c == '\0' )
				{
					// End of file.
					record.Add( reader.GetField() );
					return true;
				}

				reader.GetChar( out c );
			}
		}

		/// <summary>
		/// Reads until the field is not quoted and a delimeter is found.
		/// </summary>
	    protected virtual bool ReadQuotedField()
		{
			var inQuotes = true;
			reader.SetFieldStart();

			while( true )
			{
				// "a""b"

				reader.GetChar( out c );
				if( c == Configuration.Quote )
				{
					inQuotes = !inQuotes;

					if( !inQuotes )
					{
						// Add an offset for the quote.
						reader.AppendField( 1 );
					}

					continue;
				}

				if( inQuotes && ( c == '\r' || c == '\n' ) )
				{
					ReadLineEnding();
					currentRawRow++;
				}

				if( !inQuotes )
				{
					if( c == Configuration.Delimiter[0] )
					{
						if( ReadDelimiter() )
						{
							// Add an extra offset because of the end quote.
							record.Add( reader.GetField( Configuration.Delimiter.Length ) );
							return false;
						}
					}
					else if( c == '\r' || c == '\n' )
					{
						record.Add( reader.GetField( 1 ) );
						if( ReadLineEnding() )
						{
							reader.SetFieldStart();
						}
						return true;
					}
				}
			}
		}

		/// <summary>
		/// Reads until the delimeter is done.
		/// </summary>
		/// <returns>True if a delimiter was read. False if the sequence of
		/// chars ended up not being the delimiter.</returns>
	    protected virtual bool ReadDelimiter()
	    {
			if( c != Configuration.Delimiter[0] )
			{
				throw new InvalidOperationException( "Tried reading a delimiter when the first delimiter char didn't match the current char." );
			}

			if( Configuration.Delimiter.Length == 1 )
			{
				return true;
			}

			for( var i = 1; i < Configuration.Delimiter.Length; i++ )
			{
				reader.GetChar( out c );
				if( c != Configuration.Delimiter[i] )
				{
					return false;
				}
			}

			return true;
	    }

		/// <summary>
		/// Reads until the line ending is done.
		/// </summary>
		/// <returns>True if more chars were read, otherwise false.</returns>
	    protected virtual bool ReadLineEnding()
		{
		    if( c == '\r' )
		    {
			    reader.GetChar( out c );
			    if( c == '\n' )
			    {
					return true;
			    }
		    }

			return false;
	    }
	}
}
