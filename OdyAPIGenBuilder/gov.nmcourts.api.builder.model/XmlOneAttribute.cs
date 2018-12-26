/**************************************

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
using static OdyAPIGenBuilder.gov.nmcourts.api.builder.main.Constants;

namespace OdyAPIGenBuilder.gov.nmcourts.api.builder.model
{
    class XmlOneAttribute
    {
        private XmlAddReplace addReplace;
        private String name;
        private String value;
        private String prefix;

        public XmlOneAttribute(XmlAddReplace addReplace, String name, String value)
        {
            this.AddReplace = addReplace;
            this.Name = name;
            this.Value = value;
            this.Prefix = null;
        }

        public XmlOneAttribute(XmlAddReplace addReplace, String name, String value, String prefix)
        {
            this.AddReplace = addReplace;
            this.Name = name;
            this.Value = value;
            this.Prefix = prefix;
        }

        public XmlAddReplace AddReplace { get => addReplace; set => addReplace = value; }
        public string Name { get => name; set => name = value; }
        public string Value { get => value; set => this.value = value; }
        public string Prefix { get => prefix; set => prefix = value; }

        override
        public String ToString()
        {
            return "XmlOneAttribute [addReplace=" + AddReplace + ", name=" + name + ", value=" + value + ", prefix=" + prefix
                    + "]";
        }
    }
}
