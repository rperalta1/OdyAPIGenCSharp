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
using System.Collections;

namespace OdyAPIGenBuilder.gov.nmcourts.api.builder.model
{
    class XjcEpisode
    {
        private String xsdFile;
        private String prefix;
        private ArrayList predecessors = new ArrayList();

        public XjcEpisode(String xsdFile, String prefix, ArrayList predecessors)
        {
            this.xsdFile = xsdFile;
            this.prefix = prefix;
            this.predecessors = predecessors;
        }


        public string XsdFile { get => xsdFile; set => xsdFile = value; }
        public string Prefix { get => prefix; set => prefix = value; }
        public ArrayList Predecessors { get => predecessors; set => predecessors = value; }

        override
        public String ToString()
        {
            return "Episode [xsdFile=" + xsdFile + ", prefix=" + prefix + ", predecessors=" + predecessors + "]";
        }
    }
}
