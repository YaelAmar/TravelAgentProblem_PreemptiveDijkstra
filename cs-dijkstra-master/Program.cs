
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace cs_dijkstra
{
    class Program
    {
        // Our dictionary of nodes. Allows us to quickly change a nodes value
        // through its name (the key).
        static Dictionary<string, Node> nodeDict = new Dictionary<string, Node>();

        // The list of routes.
        static List<Route> routes = new List<Route>();

        // This set allows us to quickly check which nodes we have already
        // visited.
        static HashSet<string> unvisited = new HashSet<string>();

        const string graphFilePath = "./graph.txt";

        static void Main(string[] args)
        {
            try
            {
                initGraph();
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }

            Console.Clear();
            PrintOverview();

            var (startNode, destNode) = GetStartAndEnd();
            
            // Set our start node. The start node has to have a value
            // of 0 because we're already there.
            nodeDict[startNode].Value = 0;

            var queue = new PrioQueue();
            queue.AddNodeWithPriority(nodeDict[startNode]);

            // Do the calculations to find the shortest path to every node
            // in the graph starting from our starting node.
            CheckNode(queue, destNode);

            // Print out the result
            PrintShortestPath(startNode, destNode);
        }

        private static void initGraph()
        {
            if (!File.Exists(graphFilePath))
            {
                throw new FileNotFoundException("File not found");
            }

            using (var fileStream = File.OpenRead(graphFilePath))
            {
                using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, 128))
                {
                    String line;
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        var values = line.Split(",");
                        var (from, to, distance) = (values[0], values[1], double.Parse(values[2]));
                        if (!nodeDict.ContainsKey(from)) { nodeDict.Add(from, new Node(from)); }
                        if (!nodeDict.ContainsKey(to)) { nodeDict.Add(to, new Node(to)); }
                        unvisited.Add(from);
                        unvisited.Add(to);

                        routes.Add(new Route(from, to, distance));
                    }
                }
            }
        }

        private static void PrintOverview()
        {
            Console.WriteLine("Nodes:");
            foreach (Node node in nodeDict.Values)
            {
                Console.WriteLine("\t" + node.Name);
            }

            Console.WriteLine("\nRoutes:");
            foreach (Route route in routes)
            {
                Console.WriteLine("\t" + route.From + " -> " + route.To + " Distance: " + route.Distance);
            }
        }

        // Get user input for the start and destionation nodes.
        private static (string, string) GetStartAndEnd()
        {
            Console.WriteLine("\nEnter the start node: ");
            string startNode = Console.ReadLine();
            Console.WriteLine("Enter the destination node: ");
            string destNode = Console.ReadLine();
            return (startNode, destNode);
        }

        // Called for each node in the graph and iterates over its directly
        // connected nodes. The function always handles the node that
        // currently has the highest priority in our queue.
        // So this function checks any directly connected  node and compares
        // the value it currently holds (the shortest path we know to it) is
        // bigger than the distance of the path through the node we're
        // currently checking.
        // If it is, we just found a shorter path to it and we update its
        // 'shortest path' value and also update its previous node to the
        // one we're currently processing.
        // Every directly connected node that we find we also add to the queue
        // (which is sorted by distance), if it's not already in the queue.
        // After we're finished 
        private static void CheckNode(PrioQueue queue, string destinationNode)
        {
            // If there are no nodes left to check in our queue, we're done.
            if (queue.Count == 0)
            {
                return;
            }

            foreach (var route in routes.FindAll(r => r.From == queue.First.Value.Name))
            {
                // Skip routes to nodes that have already been visited.
                if (!unvisited.Contains(route.To))
                {
                    continue;
                }

                double travelledDistance = nodeDict[queue.First.Value.Name].Value + route.Distance;

                // We only look at nodes we haven't visited yet and we only
                // update the node's values if the distance of the path we're
                // currently checking is shorter than the one we found before.
                if (travelledDistance < nodeDict[route.To].Value)
                {
                    nodeDict[route.To].Value = travelledDistance;
                    nodeDict[route.To].PreviousNode = nodeDict[queue.First.Value.Name];
                }

                // We don't add the 'to' node to the queue if it has already been
                // visited and we don't allow duplicates.
                if (!queue.HasLetter(route.To))
                {
                    queue.AddNodeWithPriority(nodeDict[route.To]);
                }
            }
            unvisited.Remove(queue.First.Value.Name);
            queue.RemoveFirst();

            CheckNode(queue, destinationNode);
        }

        // Starts with the destination node and basically adds all the nodes'
        // respective previous nodes to a list, which is then reversed and
        // printed out.
        private static void PrintShortestPath(string startNode, string destNode)
        {
            var pathList = new List<String> { destNode } ;

            Node currentNode = nodeDict[destNode];
            while (currentNode != nodeDict[startNode])
            {
                pathList.Add(currentNode.PreviousNode.Name);
                currentNode = currentNode.PreviousNode;
            }

            pathList.Reverse();
            for (int i = 0; i < pathList.Count; i++)
            {
                Console.Write(pathList[i] + (i < pathList.Count - 1 ? " -> " : "\n"));
            }
            Console.WriteLine("Overall distance: " + nodeDict[destNode].Value);
        }
    }
}
