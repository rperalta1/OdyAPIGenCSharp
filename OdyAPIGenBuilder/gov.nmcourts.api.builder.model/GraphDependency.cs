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
    class GraphDependency
    {
        private String node;
        private String predecesor;

        public string Node { get => node; set => node = value; }
        public string Predecesor { get => predecesor; set => predecesor = value; }

        public GraphDependency(String node, String predecesor)
        {
            this.node = node;
            this.predecesor = predecesor;
        }

        override
        public String ToString()
        {
            return "Dependency [node=" + node + ", predecesor=" + predecesor + "]";
        }
    }
}
