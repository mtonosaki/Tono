// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using Tono.Logic;

namespace UnitTestLogic
{
    [TestClass]
    public class TspResolverTest
    {
        public class Node
        {
            public string Name { get; set; }
            public override int GetHashCode()
            {
                return Name.GetHashCode();
            }

            public override string ToString()
            {
                return Name;
            }

            public override bool Equals(object obj)
            {
                if (obj is Node tar)
                {
                    return Name.Equals(tar.Name);
                }
                else
                {
                    return false;
                }
            }
        }

        [TestMethod]
        public void Test001()
        {
            var tsp = new TspResolverLoop<Node>
            {
                List = new List<Node>
                {
                    new Node{ Name = "A" },
                    new Node{ Name = "B" },
                    new Node{ Name = "C" },
                    new Node{ Name = "D" },
                },
                CostCaluclator = (nodex, nodey, stage) =>
                {
                    switch (nodex.Name + nodey.Name)
                    {
                        case "AB": case "BA": return 6.0;
                        case "AC": case "CA": return 5.0;
                        case "AD": case "DA": return 5.0;
                        case "BC": case "CB": return 7.0;
                        case "BD": case "DB": return 4.0;
                        case "CD": case "DC": return 3.0;
                        default: throw new NotImplementedException();
                    }
                },
            };
            tsp.Start();
            var route = string.Join("", tsp.List.Select(a => a.ToString()));
            Assert.AreEqual(route, "ABDC"); // The minimum route is ABDCA (loop)

            var cost = 0.0;
            for (var i = 0; i < tsp.List.Count; i++)
            {
                var nodeX = tsp.List[i % tsp.List.Count];
                var nodeY = tsp.List[(i + 1) % tsp.List.Count];
                cost += tsp.CostCaluclator(nodeX, nodeY, TspResolverBase<Node>.CaluclationStage.Normal);
            }
            Assert.AreEqual(Math.Round(cost, 8), 18.0);
        }
    }
}
