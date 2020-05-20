﻿/*  Copyright (C) 2008 - 2011 Jordan Marr

This library is free software; you can redistribute it and/or
modify it under the terms of the GNU Lesser General Public
License as published by the Free Software Foundation; either
version 3 of the License, or (at your option) any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public
License along with this library. If not, see <http://www.gnu.org/licenses/>. */

using System;
using System.Data;
using System.Data.OleDb;

namespace Marr.Data.Parameters
{
    public class OleDbTypeBuilder : IDbTypeBuilder
    {
        public Enum GetDbType(Type type)
        {
            if (type == typeof(String))
                return OleDbType.VarChar;

            if (type == typeof(Int32))
                return OleDbType.Integer;

            if (type == typeof(Decimal))
                return OleDbType.Decimal;

            if (type == typeof(DateTime))
                return OleDbType.DBTimeStamp;

            if (type == typeof(Boolean))
                return OleDbType.Boolean;

            if (type == typeof(Int16))
                return OleDbType.SmallInt;

            if (type == typeof(Int64))
                return OleDbType.BigInt;

            if (type == typeof(Double))
                return OleDbType.Double;

            if (type == typeof(Byte))
                return OleDbType.Binary;

            if (type == typeof(Byte[]))
                return OleDbType.VarBinary;

            if (type == typeof(Guid))
                return OleDbType.Guid;

            return OleDbType.Variant;
        }

        public void SetDbType(IDbDataParameter param, Enum dbType)
        {
            var oleDbParam = (OleDbParameter)param;
            oleDbParam.OleDbType = (OleDbType)dbType;
        }
    }
}
