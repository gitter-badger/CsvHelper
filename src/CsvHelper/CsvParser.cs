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
		private FieldReader reader;
		private bool disposed;
	    private int currentRow;
	    private int currentRawRow;
	    private int c = -1;
	    private bool hasExcelSeparatorBeenRead;

		public virtual CsvConfiguration Configuration { get; }

	    public virtual int FieldCount { get; protected set; }

	    public virtual long CharPosition => reader.CharPosition;

	    public virtual long BytePosition => reader.BytePosition;

		public virtual int Row => currentRow;

		public virtual int RawRow => currentRawRow;

	    public virtual string RawRecord => reader.RawRecord;

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

		    this.reader = new FieldReader( reader, configuration );
		    Configuration = configuration;
	    }

		public virtual string[] Read()
		{
			CheckDisposed();

			try
			{
				if( Configuration.HasExcelSeparator && !hasExcelSeparatorBeenRead )
				{
					ReadExcelSeparator();
				}

				reader.ClearRawRecord();

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
		    currentRow++;
		    currentRawRow++;

			while( true )
			{
				c = reader.GetChar();

			    if( c == -1 )
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

				if( c == Configuration.Quote && !Configuration.IgnoreQuotes )
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
				    reader.SetFieldStart();
					return;
			    }

			    if( c == -1 )
			    {
				    return;
			    }

				c = reader.GetChar();
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
				c = reader.GetChar();
			}

			while( true )
			{
				if( c == Configuration.Delimiter[0] )
				{
					// End of field.
					if( ReadDelimiter() )
					{
						// Set the end of the field to the char before the delimiter.
						reader.SetFieldEnd( -1 );
						record.Add( reader.GetField() );
						return false;
					}
				}
				else if( c == '\r' || c == '\n' )
				{
					// End of line.
					reader.SetFieldEnd( -1 );
					var offset = ReadLineEnding();
					record.Add( reader.GetField() );
					reader.SetFieldStart( offset );
					return true;
				}
				else if( c == -1 )
				{
					// End of file.
					reader.SetFieldEnd();
					record.Add( reader.GetField() );
					return true;
				}

				c = reader.GetChar();
			}
		}

		/// <summary>
		/// Reads until the field is not quoted and a delimeter is found.
		/// </summary>
		/// <returns>True if the end of the line was found, otherwise false.</returns>
		protected virtual bool ReadQuotedField()
		{
			var inQuotes = true;
			// Set the start of the field to after the quote.
			reader.SetFieldStart();
			int cPrev;

			while( true )
			{
				// 1,"2" ,3

				cPrev = c;
				c = reader.GetChar();
				if( c == Configuration.Quote )
				{
					inQuotes = !inQuotes;

					if( !inQuotes )
					{
						// Add an offset for the quote.
						reader.SetFieldEnd( -1 );
						reader.AppendField();
						reader.SetFieldStart();
					}

					continue;
				}

				if( inQuotes )
				{
					if( c == '\r' || c == '\n' )
					{
						ReadLineEnding();
						currentRawRow++;
					}

					if( c == -1 )
					{
						reader.SetFieldEnd();
						record.Add( reader.GetField() );
						return true;
					}
				}

				if( !inQuotes )
				{
					if( c == Configuration.Delimiter[0] )
					{
						if( ReadDelimiter() )
						{
							// Add an extra offset because of the end quote.
							reader.SetFieldEnd( -1 );
							record.Add( reader.GetField() );
							return false;
						}
					}
					else if( c == '\r' || c == '\n' )
					{
						reader.SetFieldEnd( -1 );
						var offset = ReadLineEnding();
						record.Add( reader.GetField() );
						reader.SetFieldStart( offset );
						return true;
					}
					else if( cPrev == Configuration.Quote )
					{
						// We're out of quotes. Read the reset of
						// the field like a normal field.
						return ReadField();
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
				c = reader.GetChar();
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
	    protected virtual int ReadLineEnding()
		{
			var fieldStartOffset = 0;
			// TODO: Returning the bool might be pointless now. Check on this.
		    if( c == '\r' )
		    {
				c = reader.GetChar();
				if( c != '\n' )
				{
					// The start needs to be moved back.
					fieldStartOffset--;
			    }
		    }

			return fieldStartOffset;
		}

		/// <summary>
		/// Reads the Excel seperator and sets it to the delimiter.
		/// </summary>
		protected virtual void ReadExcelSeparator()
		{
			// sep=delimiter
			var sepLine = reader.Reader.ReadLine();
			if( sepLine != null )
			{
				Configuration.Delimiter = sepLine.Substring( 4 );
			}

			hasExcelSeparatorBeenRead = true;
		}
	}
}
