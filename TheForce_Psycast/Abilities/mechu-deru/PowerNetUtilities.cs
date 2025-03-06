using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace TheForce_Psycast.Abilities.Mechu_deru
{
    public static class PowerNetUtility
    {
        public static HashSet<Building> GetConnectedBuildings(CompPower powerComp)
        {
            HashSet<Building> connectedBuildings = new HashSet<Building>();

            if (powerComp?.PowerNet == null)
            {
                return connectedBuildings;
            }

            // Add buildings from transmitters
            foreach (var transmitter in powerComp.PowerNet.transmitters)
            {
                if (transmitter.parent is Building building)
                {
                    connectedBuildings.Add(building);
                }
            }

            // Add buildings from connectors
            foreach (var connector in powerComp.PowerNet.connectors)
            {
                if (connector.parent is Building building)
                {
                    connectedBuildings.Add(building);
                }
            }

            return connectedBuildings;
        }
    }
}
