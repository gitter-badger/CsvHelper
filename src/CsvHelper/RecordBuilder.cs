using System;

namespace CsvHelper
{
    public class RecordBuilder
    {
	    private const int DEFAULT_CAPACITY = 16;
	    private string[] record;
	    private int position;

	    public int Length => position;

	    public int Capacity { get; protected set; }

	    public RecordBuilder() : this( DEFAULT_CAPACITY ) { }

	    public RecordBuilder( int capacity )
	    {
		    Capacity = capacity > 0 ? capacity : DEFAULT_CAPACITY;

		    record = new string[Capacity];
	    }

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

	    public virtual RecordBuilder Clear()
	    {
		    position = 0;

		    return this;
	    }

	    public virtual string[] ToArray()
	    {
		    var array = new string[position];
		    Array.Copy( record, array, position );

			return array;
	    }
    }
}
