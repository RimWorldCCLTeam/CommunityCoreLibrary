using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using Verse;

namespace CommunityCoreLibrary
{
    public class FactoryWithProduct : Thing
    {
        private ThingDef thingToProduce;
        private Building_AutomatedFactory factory;
        private int ammountToProduce;

        public FactoryWithProduct(Building_AutomatedFactory factory, ThingDef thingToProduce)
        {
            this.factory = factory;
            this.thingToProduce = thingToProduce;
        }

        public int AmmountToProduce
        {
            get { return ammountToProduce; }
            set { ammountToProduce = value; }
        }

        public ThingDef ThingToProduce
        {
            get { return thingToProduce; }
            set { thingToProduce = value; }
        }
        public Building_AutomatedFactory Factory
        {
            get { return factory; }
            set { factory = value; }
        }

        public int AmmountCanProduce()
        {
            //todo add can produce number function to factory and call it here.
            return 1;
        }

    }
}