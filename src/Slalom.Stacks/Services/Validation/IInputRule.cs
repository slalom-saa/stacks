﻿/* 
 * Copyright (c) Stacks Contributors
 * 
 * This file is subject to the terms and conditions defined in
 * the LICENSE file, which is part of this source code package.
 */

using Slalom.Stacks.Validation;

namespace Slalom.Stacks.Services.Validation
{
    /// <summary>
    /// Validates and object instance using input rules.
    /// </summary>
    /// <typeparam name="TValue">The type to validate.</typeparam>
    public interface IInputRule<in TValue> : IValidate<TValue>
    {
    }
}