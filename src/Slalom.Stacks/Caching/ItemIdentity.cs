/* 
 * Copyright (c) Stacks Contributors
 * 
 * This file is subject to the terms and conditions defined in
 * the LICENSE file, which is part of this source code package.
 */

using Slalom.Stacks.Domain;
using Slalom.Stacks.Search;

namespace Slalom.Stacks.Caching
{
    internal static class ItemIdentity
    {
        public static string GetIdentity(object instance)
        {
            var entity = instance as IAggregateRoot;
            if (entity != null)
            {
                return entity.Id;
            }
            var result = instance as ISearchResult;
            if (result != null)
            {
                return result.Id.ToString();
            }
            return instance.GetHashCode().ToString();
        }
    }
}