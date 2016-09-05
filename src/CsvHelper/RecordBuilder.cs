using System;

namespace CsvHelper
{
	/// <summary>
	/// Builds CSV records.
	/// </summary>
    public class RecordBuilder
    {
	    private const int DEFAULT_CAPACITY = 16;
	    private string[] record;
	    private int position;

		/// <summary>
		/// The number of records.
		/// </summary>
	    public int Length => position;

		/// <summary>
		/// The total record capacity.
		/// </summary>
	    public int Capacity { get; protected set; }

		/// <summary>
		/// Creates a new <see cref="RecordBuilder"/> using defaults.
		/// </summary>
	    public RecordBuilder() : this( DEFAULT_CAPACITY ) { }

		/// <summary>
		/// Creatse a new <see cref="RecordBuilder"/> using the given capacity.
		/// </summary>
		/// <param name="capacity">The initial capacity.</param>
	    public RecordBuilder( int capacity )
	    {
		    Capacity = capacity > 0 ? capacity : DEFAULT_CAPACITY;

		    record = new string[Capacity];
	    }

		/// <summary>
		/// Adds a new field to the <see cref="RecordBuilder"/>.
		/// </summary>
		/// <param name="field">The field to add.</param>
		/// <returns>The current instance of the <see cref="RecordBuilder"/>.</returns>
		public virtual RecordBuilder Add( string field )
	    {
			if( position == record.Length )
			{
				Capacity = Capacity * 2;
				Array.Resize( ref record, Capacity );
			}

			record[position] = field;
		    position++;

		    return this;
	    }

		/// <summary>
		/// Clears the records.
		/// </summary>
		/// <returns>The current instance of the <see cref="RecordBuilder"/>.</returns>
	    public virtual RecordBuilder Clear()
	    {
		    position = 0;

		    return this;
	    }

		/// <summary>
		/// Returns the record as an <see cref="T:string[]"/>.
		/// </summary>
		/// <returns>The record as an <see cref="T:string[]"/>.</returns>
		public virtual string[] ToArray()
	    {
		    var array = new string[position];
		    Array.Copy( record, array, position );

			return array;
	    }
    }
}
