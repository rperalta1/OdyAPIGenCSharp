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
using OdyAPIGenBuilder.gov.nmcourts.api.builder.model;
using System;
using System.Collections;
using System.Collections.Generic;

namespace OdyAPIGenBuilder.gov.nmcourts.api.builder.main
{
    class DiGraph
    {
        List<GraphVertex> nodes = new List<GraphVertex>();
        ArrayList predecesors;

        public ArrayList Predecesors { get => predecesors; set => predecesors = value; }

        bool Contains(String vertex)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].GetVertex().ToLower().Equals(vertex.ToLower()))
                {
                    return true;
                }
            }
            return false;
        }

        public void CreateNode(String vertex)
        {
            nodes.Add(new GraphVertex(vertex));
        }

        public int GetIndexNode(String vertex)
        {
            for (int index = 0; index < nodes.Count; index++)
            {
                if (nodes[index].GetVertex().ToLower().Equals(vertex.ToLower()))
                    return index;
            }
            return -1;
        }

        public void AddNode(int index, String to)
        {
            if (to == null || to.Length == 0) return;
            if (index >= 0)
            {
                GraphVertex vertexData = nodes[index];
                vertexData.AddAdj(to);
            }
        }

        public List<GraphVertex> GetNodes()
        {
            return nodes;
        }

        public void SetAdjNodes(List<GraphVertex> adjNodes)
        {
            this.nodes = adjNodes;
        }

        public void AddDiGraph(String vertex)
        {
            if (vertex == null || vertex.Length == 0)
                return;
            if (Contains(vertex)) return;
            else CreateNode(vertex);
        }

        protected void AddDiGraph(String from, String to)
        {
            this.AddDiGraph(from);
            this.AddDiGraph(to);
            AddNode(GetIndexNode(from), to);
        }

        protected void FindRoots(List<int> rootIndices)
        {
            int index = 0;
            foreach (GraphVertex node in nodes)
            {
                if (node.Neighbors.Count == 0)
                {
                    rootIndices.Add(index);
                }
                index++;
            }
        }

        private bool IsAdjNeighbors(GraphVertex node, List<String> packages)
        {
            if (node.Neighbors.Count == 0)
                return false;
            if (IsNodeInPackages(node.GetVertex().ToString(), packages))
                return false;
            if (CheckDependencies(node.Neighbors, packages))
                return true;
            return false;
        }

        private bool IsNodeInPackages(String node, List<String> packages)
        {
            foreach (String pack in packages)
            {
                if (node.ToLower().Equals(pack.ToLower()))
                    return true;
            }
            return false;
        }

        private bool CheckDependencies(ArrayList neighbors, List<String> packages)
        {
            foreach (String neighbor in neighbors)
            {
                if (!IsNodeInPackages(neighbor, packages))
                    return false;
            }
            return true;
        }

        private void AddPackage(GraphVertex vertex, List<String> packages)
        {
            packages.Add(vertex.GetVertex());
        }

        public void ProcessGraph(List<String> packages)
        {
            List<int> rootIndices = new List<int>();
            FindRoots(rootIndices);

            foreach (int rootIndex in rootIndices)
            {
                int lastNumPackages = 0;

                AddPackage(nodes[rootIndex], packages);

                while (packages.Count != lastNumPackages)
                {
                    lastNumPackages = packages.Count;
                    foreach (GraphVertex node in nodes)
                    {
                        if (IsAdjNeighbors(node, packages) && node.Neighbors.Count > 0)
                        {
                            AddPackage(node, packages);
                            break;
                        }
                    }
                }
            }
        }

        public ArrayList GetAllPredecessors(String targetVertex)
        {
            ResetVisited();
            Dfs(FindVertex(targetVertex));
            predecesors.RemoveAt(0);
            return predecesors;
        }

        private void ResetVisited()
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i].Visited = false;
            }
            predecesors = null;
            predecesors = new ArrayList();
        }

        private GraphVertex FindVertex(String targetVertex)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].Node.ToUpper().Equals(targetVertex.ToUpper()))
                {
                    return nodes[i];
                }
            }
            return null;
        }

        private void Dfs(GraphVertex node)
        {
            predecesors.Add((String) node.GetVertex());
            ArrayList neighbours = node.Neighbors;
            node.Visited = true;
            for (int i = 0; i < neighbours.Count; i++)
            {
                GraphVertex n = FindVertex((String) neighbours[i]);
                if (n != null && !n.Visited)
                {
                    Dfs(n);
                }
            }
        }
    }
}
