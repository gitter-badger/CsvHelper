﻿// Copyright 2009-2015 Josh Close and Contributors
// This file is a part of CsvHelper and is dual licensed under MS-PL and Apache 2.0.
// See LICENSE.txt for details or visit http://www.opensource.org/licenses/ms-pl.html for MS-PL and http://opensource.org/licenses/Apache-2.0 for Apache 2.0.
// http://csvhelper.com
#if !NET_2_0
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using CsvHelper.TypeConversion;

namespace CsvHelper.Configuration
{
	/// <summary>
	/// Mapping info for a property to a CSV field.
	/// </summary>
	[DebuggerDisplay( "Names = {string.Join(\",\", Data.Names)}, Index = {Data.Index}, Ignore = {Data.Ignore}, Property = {Data.Property}, TypeConverter = {Data.TypeConverter}" )]
	public class CsvPropertyMap
	{
		/// <summary>
		/// Gets the property map data.
		/// </summary>
		public CsvPropertyMapData Data { get; }

		/// <summary>
		/// Creates a new <see cref="CsvPropertyMap"/> instance using the specified property.
		/// </summary>
		public CsvPropertyMap( PropertyInfo property )
		{
			Data = new CsvPropertyMapData( property )
			{
				// Set some defaults.
				TypeConverter = TypeConverterFactory.GetConverter( property.PropertyType )
			};
			Data.Names.Add( property.Name );
		}

		/// <summary>
		/// When reading, is used to get the field
		/// at the index of the name if there was a
		/// header specified. It will look for the
		/// first name match in the order listed.
		/// When writing, sets the name of the 
		/// field in the header record.
		/// The first name will be used.
		/// </summary>
		/// <param name="names">The possible names of the CSV field.</param>
		public virtual CsvPropertyMap Name( params string[] names )
		{
			if( names == null || names.Length == 0 )
			{
				throw new ArgumentNullException( nameof( names ) );
			}

			Data.Names.Clear();
			Data.Names.AddRange( names );
			Data.IsNameSet = true;
			return this;
		}

		/// <summary>
		/// When reading, is used to get the 
		/// index of the name used when there 
		/// are multiple names that are the same.
		/// </summary>
		/// <param name="index">The index of the name.</param>
		public virtual CsvPropertyMap NameIndex( int index )
		{
			Data.NameIndex = index;
			return this;
		}

		/// <summary>
		/// When reading, is used to get the field at
		/// the given index. When writing, the fields
		/// will be written in the order of the field
		/// indexes.
		/// </summary>
		/// <param name="index">The index of the CSV field.</param>
		/// <param name="indexEnd">The end index used when mapping to an <see cref="IEnumerable"/> property.</param>
		public virtual CsvPropertyMap Index( int index, int indexEnd = -1 )
		{
			Data.Index = index;
			Data.IsIndexSet = true;
			Data.IndexEnd = indexEnd;
			return this;
		}

		/// <summary>
		/// Ignore the property when reading and writing.
		/// </summary>
		public virtual CsvPropertyMap Ignore()
		{
			Data.Ignore = true;
			return this;
		}

		/// <summary>
		/// Ignore the property when reading and writing.
		/// </summary>
		/// <param name="ignore">True to ignore, otherwise false.</param>
		public virtual CsvPropertyMap Ignore( bool ignore )
		{
			Data.Ignore = ignore;
			return this;
		}

		/// <summary>
		/// The default value that will be used when reading when
		/// the CSV field is empty.
		/// </summary>
		/// <param name="defaultValue">The default value.</param>
		public virtual CsvPropertyMap Default( object defaultValue )
		{
			Data.Default = defaultValue;
			return this;
		}

		/// <summary>
		/// Specifies the <see cref="TypeConverter"/> to use
		/// when converting the property to and from a CSV field.
		/// </summary>
		/// <param name="typeConverter">The TypeConverter to use.</param>
		public virtual CsvPropertyMap TypeConverter( ITypeConverter typeConverter )
		{
			Data.TypeConverter = typeConverter;
			return this;
		}

		/// <summary>
		/// Specifies the <see cref="TypeConverter"/> to use
		/// when converting the property to and from a CSV field.
		/// </summary>
		/// <typeparam name="T">The <see cref="System.Type"/> of the 
		/// <see cref="TypeConverter"/> to use.</typeparam>
		public virtual CsvPropertyMap TypeConverter<T>() where T : ITypeConverter
		{
			TypeConverter( ReflectionHelper.CreateInstance<T>() );
			return this;
		}

		/// <summary>
		/// Specifies an expression to be used to convert data in the
		/// row to the property.
		/// </summary>
		/// <typeparam name="T">The type of the property that will be set.</typeparam>
		/// <param name="convertExpression">The convert expression.</param>
		public virtual CsvPropertyMap ConvertUsing<T>( Func<ICsvReaderRow, T> convertExpression )
		{
			var returnType = typeof( T );
			if( !Data.Property.PropertyType.IsAssignableFrom( returnType ) )
			{
				throw new CsvConfigurationException( $"ConvertUsing return type '{returnType.FullName}' cannot be assigned to property type '{Data.Property.PropertyType.FullName}'." );
			}

			Data.ConvertExpression = (Expression<Func<ICsvReaderRow, T>>)( x => convertExpression( x ) );
			return this;
		}

		/// <summary>
		/// The <see cref="CultureInfo"/> used when type converting.
		/// This will override the global <see cref="CsvConfiguration.CultureInfo"/>
		/// setting.
		/// </summary>
		/// <param name="cultureInfo">The culture info.</param>
		public virtual CsvPropertyMap TypeConverterOption( CultureInfo cultureInfo )
		{
			Data.TypeConverterOptions.CultureInfo = cultureInfo;
			return this;
		}

		/// <summary>
		/// The <see cref="DateTimeStyles"/> to use when type converting.
		/// This is used when doing any <see cref="DateTime"/> conversions.
		/// </summary>
		/// <param name="dateTimeStyle">The date time style.</param>
		public virtual CsvPropertyMap TypeConverterOption( DateTimeStyles dateTimeStyle )
		{
			Data.TypeConverterOptions.DateTimeStyle = dateTimeStyle;
			return this;
		}

		/// <summary>
		/// The <see cref="NumberStyles"/> to use when type converting.
		/// This is used when doing any number conversions.
		/// </summary>
		/// <param name="numberStyle"></param>
		public virtual CsvPropertyMap TypeConverterOption( NumberStyles numberStyle )
		{
			Data.TypeConverterOptions.NumberStyle = numberStyle;
			return this;
		}

		/// <summary>
		/// The string format to be used when type converting.
		/// </summary>
		/// <param name="format">The format.</param>
		public virtual CsvPropertyMap TypeConverterOption( string format )
		{
			Data.TypeConverterOptions.Format = format;
			return this;
		}

		/// <summary>
		/// The string values used to represent a boolean when converting.
		/// </summary>
		/// <param name="isTrue">A value indicating whether true values or false values are being set.</param>
		/// <param name="booleanValues">The string boolean values.</param>
		public virtual CsvPropertyMap TypeConverterOption( bool isTrue, params string[] booleanValues )
		{
			return TypeConverterOption( isTrue, true, booleanValues );
		}

		/// <summary>
		/// The string values used to represent a boolean when converting.
		/// </summary>
		/// <param name="isTrue">A value indicating whether true values or false values are being set.</param>
		/// <param name="clearValues">A value indication if the current values should be cleared before adding the new ones.</param>
		/// <param name="booleanValues">The string boolean values.</param>
		public virtual CsvPropertyMap TypeConverterOption( bool isTrue, bool clearValues, params string[] booleanValues )
		{
			if( isTrue )
			{
				if( clearValues )
				{
					Data.TypeConverterOptions.BooleanTrueValues.Clear();
				}
				Data.TypeConverterOptions.BooleanTrueValues.AddRange( booleanValues );
			}
			else
			{
				if( clearValues )
				{
					Data.TypeConverterOptions.BooleanFalseValues.Clear();
				}
				Data.TypeConverterOptions.BooleanFalseValues.AddRange( booleanValues );
			}
			return this;
		}
	}
}
#endif // !NET_2_0
