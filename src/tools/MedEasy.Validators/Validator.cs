﻿using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MedEasy.Validators
{
    /// <summary>
    /// Static class
    /// </summary>
    /// <typeparam name="T">Type of element to validate</typeparam>
    public sealed class Validator<T> : AbstractValidator<T>
    {
       
        private Validator()
        {}

        private static readonly Lazy<Validator<T>> _lazy = new Lazy<Validator<T>>(() => new Validator<T>());

        /// <summary>
        /// Gets the default validator instance for the specified type
        /// </summary>
        public static Validator<T> Default => _lazy.Value;
    }
}
