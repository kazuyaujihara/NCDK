



// .NET Framework port by Kazuya Ujihara
// Copyright (C) 2016-2017  Kazuya Ujihara <ujihara.kazuya@gmail.com>

/* Copyright (C) 1997-2007  Christoph Steinbeck <steinbeck@users.sf.net>
 *
 * Contact: cdk-devel@lists.sourceforge.net
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public License
 * as published by the Free Software Foundation; either version 2.1
 * of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301 USA.
 */

using System;
using System.Collections;
using System.Collections.Generic;

namespace NCDK.Default
{
    /// <summary>
    /// A sequence of ChemModels, which can, for example, be used to
    /// store the course of a reaction. Each state of the reaction would be
    /// stored in one ChemModel.    
    /// </summary>
    // @cdk.keyword animation
    // @cdk.keyword reaction
    public class ChemSequence
        : ChemObject, IChemSequence, IChemObjectListener, ICloneable
    {
        /// <summary>Array of ChemModels.</summary>
        private IList<IChemModel> chemModels = new List<IChemModel>();

        public ChemSequence()
            : base()
        {
        }

        /// <summary>
        /// Adds an chemModel to this container.
        /// </summary>
        /// <param name="chemModel">The chemModel to be added to this container</param>
        public void Add(IChemModel chemModel)
        {
            chemModels.Add(chemModel);
 
            chemModel.Listeners.Add(this);
            NotifyChanged(); 
        }

        public bool Remove(IChemModel chemModel)
        {
            var ret = chemModels.Remove(chemModel);
 
            chemModel.Listeners.Add(this);
            NotifyChanged(); 
            return ret;
        }

        public IChemModel this[int index]
        {
            get { return chemModels[index]; }

            set
            {
                chemModels[index] = value;
                 NotifyChanged();             }
        }

        public int Count => chemModels.Count;
        public bool IsReadOnly => chemModels.IsReadOnly;
        public void Clear() => chemModels.Clear();
        public bool Contains(IChemModel chemModel) => chemModels.Contains(chemModel);
        public void CopyTo(IChemModel[] array, int arrayIndex) => chemModels.CopyTo(array, arrayIndex);
        public IEnumerator<IChemModel> GetEnumerator() => chemModels.GetEnumerator();
        public int IndexOf(IChemModel chemModel) => chemModels.IndexOf(chemModel);
        public void Insert(int index, IChemModel chemModel) => chemModels.Insert(index, chemModel);
        public void RemoveAt(int index) => chemModels.RemoveAt(index);
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public override ICDKObject Clone(CDKObjectMap map)
        {
            var clone_chemModels = new List<IChemModel>();
            foreach (var chemModel in chemModels)
                clone_chemModels.Add((IChemModel)chemModel.Clone(map));
            var clone = (ChemSequence)base.Clone(map);
            clone.chemModels = clone_chemModels;
            return clone;
        }

        public new IChemSequence Clone() => (IChemSequence)Clone(new CDKObjectMap());
        object ICloneable.Clone() => Clone();

        /// <summary>
        ///  Called by objects to which this object has
        ///  registered as a listener.
        /// </summary>
        /// <param name="evt">A change event pointing to the source of the change</param>
        public void OnStateChanged(ChemObjectChangeEventArgs evt)
        {
             NotifyChanged(evt);         }
    }
}
namespace NCDK.Silent
{
    /// <summary>
    /// A sequence of ChemModels, which can, for example, be used to
    /// store the course of a reaction. Each state of the reaction would be
    /// stored in one ChemModel.    
    /// </summary>
    // @cdk.keyword animation
    // @cdk.keyword reaction
    public class ChemSequence
        : ChemObject, IChemSequence, IChemObjectListener, ICloneable
    {
        /// <summary>Array of ChemModels.</summary>
        private IList<IChemModel> chemModels = new List<IChemModel>();

        public ChemSequence()
            : base()
        {
        }

        /// <summary>
        /// Adds an chemModel to this container.
        /// </summary>
        /// <param name="chemModel">The chemModel to be added to this container</param>
        public void Add(IChemModel chemModel)
        {
            chemModels.Add(chemModel);
        }

        public bool Remove(IChemModel chemModel)
        {
            var ret = chemModels.Remove(chemModel);
            return ret;
        }

        public IChemModel this[int index]
        {
            get { return chemModels[index]; }

            set
            {
                chemModels[index] = value;
                            }
        }

        public int Count => chemModels.Count;
        public bool IsReadOnly => chemModels.IsReadOnly;
        public void Clear() => chemModels.Clear();
        public bool Contains(IChemModel chemModel) => chemModels.Contains(chemModel);
        public void CopyTo(IChemModel[] array, int arrayIndex) => chemModels.CopyTo(array, arrayIndex);
        public IEnumerator<IChemModel> GetEnumerator() => chemModels.GetEnumerator();
        public int IndexOf(IChemModel chemModel) => chemModels.IndexOf(chemModel);
        public void Insert(int index, IChemModel chemModel) => chemModels.Insert(index, chemModel);
        public void RemoveAt(int index) => chemModels.RemoveAt(index);
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public override ICDKObject Clone(CDKObjectMap map)
        {
            var clone_chemModels = new List<IChemModel>();
            foreach (var chemModel in chemModels)
                clone_chemModels.Add((IChemModel)chemModel.Clone(map));
            var clone = (ChemSequence)base.Clone(map);
            clone.chemModels = clone_chemModels;
            return clone;
        }

        public new IChemSequence Clone() => (IChemSequence)Clone(new CDKObjectMap());
        object ICloneable.Clone() => Clone();

        /// <summary>
        ///  Called by objects to which this object has
        ///  registered as a listener.
        /// </summary>
        /// <param name="evt">A change event pointing to the source of the change</param>
        public void OnStateChanged(ChemObjectChangeEventArgs evt)
        {
                    }
    }
}
