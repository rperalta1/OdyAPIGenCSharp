﻿/**************************************

    Copyright (C) 2018  
    Judicial Information Division,
    Administrative Office of the Courts,
    State of New Mexico

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Lesser General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.

***************************************/
using System;
using System.Collections;
using static OdyAPIGenBuilder.gov.nmcourts.api.builder.main.Constants;

namespace OdyAPIGenBuilder.gov.nmcourts.api.builder.model
{
    class XmlDependency
    {
        private XmlType type;
        private String prefix;
        private String name;
        private String restriction;
        private ArrayList enums = new ArrayList();

        public XmlDependency(XmlType type, String prefix, String name, String restriction, ArrayList enums)
        {
            this.Type = type;
            this.Prefix = prefix;
            this.Name = name;
            this.Restriction = restriction;
            this.Enums = enums;
        }

        public XmlType Type { get => type; set => type = value; }
        public string Prefix { get => prefix; set => prefix = value; }
        public string Name { get => name; set => name = value; }
        public string Restriction { get => restriction; set => restriction = value; }
        public ArrayList Enums { get => enums; set => enums = value; }

        override
        public String ToString()
        {
            return "XmlDependency [type=" + type + ", prefix=" + prefix + ", name=" + name + ", restriction=" + restriction
                    + ", enums=" + enums + "]";
        }
    }
}
