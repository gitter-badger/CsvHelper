﻿// Copyright 2009-2015 Josh Close and Contributors
// This file is a part of CsvHelper and is dual licensed under MS-PL and Apache 2.0.
// See LICENSE.txt for details or visit http://www.opensource.org/licenses/ms-pl.html for MS-PL and http://opensource.org/licenses/Apache-2.0 for Apache 2.0.
// http://csvhelper.com
#if !NET_2_0
using System;
using System.Reflection;

namespace CsvHelper.Configuration
{
	/// <summary>
	/// Mapping info for a reference property mapping to a class.
	/// </summary>
	public class CsvPropertyReferenceMap
	{
		private readonly CsvPropertyReferenceMapData data;

		/// <summary>
		/// Gets the property reference map data.
		/// </summary>
		public CsvPropertyReferenceMapData Data => data;

		/// <summary>
		/// Initializes a new instance of the <see cref="CsvPropertyReferenceMap"/> class.
		/// </summary>
		/// <param name="property">The property.</param>
		/// <param name="mapping">The <see cref="CsvClassMap"/> to use for the reference map.</param>
		public CsvPropertyReferenceMap( PropertyInfo property, CsvClassMap mapping )
		{
			if( mapping == null )
			{
				throw new ArgumentNullException( nameof( mapping ) );
			}

			data = new CsvPropertyReferenceMapData( property, mapping );
		}

		/// <summary>
		/// Appends a prefix to the header of each field of the reference property
		/// </summary>
		/// <param name="prefix">The prefix to be prepended to headers of each reference property</param>
		/// <returns>The current <see cref="CsvPropertyReferenceMap" /></returns>
		public CsvPropertyReferenceMap Prefix( string prefix = null )
		{
			if( string.IsNullOrEmpty( prefix ) )
			{
				prefix = data.Property.Name + ".";
			}

			data.Prefix = prefix;

			return this;
		}

		/// <summary>
		/// Get the largest index for the
		/// properties and references.
		/// </summary>
		/// <returns>The max index.</returns>
		internal int GetMaxIndex()
		{
			return data.Mapping.GetMaxIndex();
		}
	}
}
#endif // !NET_2_0
