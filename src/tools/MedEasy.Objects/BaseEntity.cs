using System;
using System.Collections.Generic;

namespace MedEasy.Objects
{
    public abstract class BaseEntity<T> : IEqualityComparer<T>, IEquatable<T>, IEntity<T>
    {
        public virtual T Id { get; }

        #region Implementation of IEqualityComparer<in BaseEntity>

        /// <summary>
        /// Détermine si les objets spécifiés sont égaux.
        /// </summary>
        /// <returns>
        /// true si les objets spécifiés sont égaux ; sinon, false.
        /// </returns>
        /// <param name="x">Premier objet de type <paramref name="T"/> à comparer.</param><param name="y">Deuxième objet de type <paramref name="T"/> à comparer.</param>
        public virtual bool Equals(T x, T y) => x.Equals(y);

        /// <summary>
        /// Retourne un code de hachage pour l'objet spécifié.
        /// </summary>
        /// <returns>
        /// Code de hachage pour l'objet spécifié.
        /// </returns>
        /// <param name="obj"><see cref="T:System.Object"/> pour lequel un code de hachage doit être retourné.</param><exception cref="T:System.ArgumentNullException">Le type de <paramref name="obj"/> est un type référence et <paramref name="obj"/> est null.</exception>
        public virtual int GetHashCode(T obj) => obj?.GetHashCode() ?? 0;

        #endregion

        #region Implementation of IEquatable<T>

        /// <summary>
        /// Indique si l'objet actuel est égal à un autre objet du même type.
        /// </summary>
        /// <returns>
        /// true si l'objet en cours est égal au paramètre <paramref name="other"/> ; sinon, false.
        /// </returns>
        /// <param name="other">Objet à comparer avec cet objet.</param>
        public virtual bool Equals(T other) => Equals(this, other);

        #endregion
    }
}
