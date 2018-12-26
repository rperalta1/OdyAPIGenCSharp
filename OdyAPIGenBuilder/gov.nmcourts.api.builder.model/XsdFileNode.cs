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

namespace OdyAPIGenBuilder.gov.nmcourts.api.builder.model
{
    class XsdFileNode
    {
        private int level;
        private int position;
        private String name;
        private String type;

        public XsdFileNode(int level, int position, String name, String type)
        {
            this.level = level;
            this.position = position;
            this.name = name;
            this.type = type;
        }

        public int Level { get => level; set => level = value; }
        public int Position { get => position; set => position = value; }
        public string Name { get => name; set => name = value; }
        public string Type { get => type; set => type = value; }

        override
        public String ToString()
        {
            return "XsdFileNode [level=" + level + ", position=" + position + ", name=" + name + ", type=" + type + "]";
        }
    }
}
