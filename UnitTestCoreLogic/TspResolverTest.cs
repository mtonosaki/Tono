// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using Tono;
using Tono.Logic;

namespace TestTonoLogic
{
    [TestClass]
    public class TspResolverTest
    {
        public class Node
        {
            public string Name { get; set; }
            public override int GetHashCode() => Name.GetHashCode();
            public override string ToString() => Name;
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
        }
    }
}
