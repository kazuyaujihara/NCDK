﻿using System.IO;

namespace NCDK.IO.Iterator
{
    class IteratingMDLConformerReader_Example
    {
        void Main()
        {
            {
                #region 
                string filename = "/Users/rguha/conf2.sdf";
                using (var srm = new FileStream(filename, FileMode.Open))
                {
                    IteratingMDLConformerReader reader = new IteratingMDLConformerReader(srm, Default.ChemObjectBuilder.Instance);
                    foreach (var cc in reader)
                    {
                        // do something 
                    }
                }
                // do something with this set of conformers
                #endregion
            }
        }
    }
}
