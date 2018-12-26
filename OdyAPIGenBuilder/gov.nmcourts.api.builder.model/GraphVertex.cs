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
    class GraphVertex
    {
        private String node;
        private Boolean visited;

        public String Node { get => node; set => node = value; }
        public ArrayList Neighbors { get; set; }
        public bool Visited { get => visited; set => visited = value; }

        public String GetVertex()
        {
            return node;
        }

        public void SetVertex(String vertex)
        {
            this.node = vertex;
        }

        public GraphVertex(String vertex)
        {
            this.Node = vertex;
            Neighbors = new ArrayList();
        }

        public void AddAdj(String to)
        {
            Neighbors.Add(to);
        }

        override
        public String ToString()
        {
            return "VertexData [node=" + Node + ", neighbors=" + Neighbors + ", visited=" + visited + "]";
        }
    }
}
