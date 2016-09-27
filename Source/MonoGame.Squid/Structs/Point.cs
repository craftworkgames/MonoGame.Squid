using System;
using System.ComponentModel;
using System.Globalization;

namespace MonoGame.Squid.Structs
{
    /// <summary>
    /// Struct Point
    /// </summary>
    [TypeConverter(typeof(PointConverter))]
    public struct Point
    {
        public static readonly Point Zero;
        private int _x;
        private int _y;

        /// <summary>
        /// Initializes a new instance of the <see cref="Point"/> struct.
        /// </summary>
        /// <param name="pt">The pt.</param>
        public Point(Point pt)
        {
            this._x = pt.X;
            this._y = pt.Y;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Point"/> struct.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        public Point(int x, int y)
        {
            this._x = x;
            this._y = y;
        }

        /// <summary>
        /// Implements the +.
        /// </summary>
        /// <param name="p1">The p1.</param>
        /// <param name="p2">The p2.</param>
        /// <returns>The result of the operator.</returns>
        public static Point operator +(Point p1, Point p2)
        {
            return new Point(p1.X + p2.X, p1.Y + p2.Y);
        }

        /// <summary>
        /// Implements the -.
        /// </summary>
        /// <param name="p1">The p1.</param>
        /// <param name="p2">The p2.</param>
        /// <returns>The result of the operator.</returns>
        public static Point operator -(Point p1, Point p2)
        {
            return new Point(p1.X - p2.X, p1.Y - p2.Y);
        }

        /// <summary>
        /// Implements the ==.
        /// </summary>
        /// <param name="p1">The p1.</param>
        /// <param name="p2">The p2.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(Point p1, Point p2)
        {
            return ((p1.X == p2.X) && (p1.Y == p2.Y));
        }

        /// <summary>
        /// Implements the !=.
        /// </summary>
        /// <param name="p1">The p1.</param>
        /// <param name="p2">The p2.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(Point p1, Point p2)
        {
            return !(p1 == p2);
        }

        /// <summary>
        /// Implements the *.
        /// </summary>
        /// <param name="a">A.</param>
        /// <param name="b">The b.</param>
        /// <returns>The result of the operator.</returns>
        public static Point operator *(Point a, int b)
        {
            return new Point(a.X * b, a.Y * b);
        }

        /// <summary>
        /// Implements the /.
        /// </summary>
        /// <param name="a">A.</param>
        /// <param name="b">The b.</param>
        /// <returns>The result of the operator.</returns>
        public static Point operator /(Point a, int b)
        {
            return new Point(a.X / b, a.Y / b);
        }

        /// <summary>
        /// Implements the *.
        /// </summary>
        /// <param name="a">A.</param>
        /// <param name="b">The b.</param>
        /// <returns>The result of the operator.</returns>
        public static Point operator *(Point a, float b)
        {
            return new Point((int)((float)a.X * b), (int)((float)a.Y * b));
        }

        /// <summary>
        /// Implements the /.
        /// </summary>
        /// <param name="a">A.</param>
        /// <param name="b">The b.</param>
        /// <returns>The result of the operator.</returns>
        public static Point operator /(Point a, float b)
        {
            return new Point((int)((float)a.X / b), (int)((float)a.Y / b));
        }

        /// <summary>
        /// Gets a value indicating whether this instance is empty.
        /// </summary>
        /// <value><c>true</c> if this instance is empty; otherwise, <c>false</c>.</value>
        [Browsable(false)]
        public bool IsEmpty
        {
            get { return ((_x == 0) && (_y == 0)); }
        }

        /// <summary>
        /// Gets or sets the x.
        /// </summary>
        /// <value>The x.</value>
        public int X
        {
            get { return _x; }
            set { _x = value; }
        }

        /// <summary>
        /// Gets or sets the y.
        /// </summary>
        /// <value>The y.</value>
        public int Y
        {
            get { return _y; }
            set { _y = value; }
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">Another object to compare to.</param>
        /// <returns><c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is Point))
                return false;

            var size = (Point)obj;
            return ((size._x == this._x) && (size._y == this._y));
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public override int GetHashCode()
        {
            return (this._x ^ this._y);
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return string.Format("{0}; {1}", X, Y);
        }

        /// <summary>
        /// Eases to.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <param name="divisor">The divisor.</param>
        /// <returns>Point.</returns>
        public static Point EaseTo(Point start, Point end, float divisor)
        {
            var x = ((float)end.X - (float)start.X) / divisor;
            var y = ((float)end.Y - (float)start.Y) / divisor;

            return start + new Point((int)x, (int)y);
        }

        static Point()
        {
            Zero = new Point();
        }
    }

    /// <summary>
    /// Converts from String to Point and vice versa.
    /// </summary>
    public class PointConverter : TypeConverter
    {
        // Methods
        /// <summary>
        /// Returns whether this converter can convert an object of the given type to the type of this converter, using the specified context.
        /// </summary>
        /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context.</param>
        /// <param name="sourceType">A <see cref="T:System.Type" /> that represents the type you want to convert from.</param>
        /// <returns>true if this converter can perform the conversion; otherwise, false.</returns>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return ((sourceType == typeof(string)) || base.CanConvertFrom(context, sourceType));
        }

        /// <summary>
        /// Returns whether this converter can convert the object to the specified type, using the specified context.
        /// </summary>
        /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context.</param>
        /// <param name="destinationType">A <see cref="T:System.Type" /> that represents the type you want to convert to.</param>
        /// <returns>true if this converter can perform the conversion; otherwise, false.</returns>
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return ((destinationType == typeof(string)) || base.CanConvertTo(context, destinationType));
        }

        /// <summary>
        /// Converts the given object to the type of this converter, using the specified context and culture information.
        /// </summary>
        /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context.</param>
        /// <param name="culture">The <see cref="T:System.Globalization.CultureInfo" /> to use as the current culture.</param>
        /// <param name="value">The <see cref="T:System.Object" /> to convert.</param>
        /// <returns>An <see cref="T:System.Object" /> that represents the converted value.</returns>
        /// <exception cref="System.ArgumentException"></exception>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (!(value is string))
                return base.ConvertFrom(context, culture, value);

            var str = ((string)value).Trim();

            if (str.Length == 0)
                return null;

            if (culture == null)
                culture = CultureInfo.CurrentCulture;

            var arr = str.Split(new char[2] { '|', ';' }, StringSplitOptions.RemoveEmptyEntries);
            var numArray = new int[arr.Length];
            var converter = TypeDescriptor.GetConverter(typeof(int));

            for (var i = 0; i < numArray.Length; i++)
                numArray[i] = (int)converter.ConvertFromString(context, culture, arr[i]);

            if (numArray.Length != 2)
                throw new ArgumentException();

            return new Point(numArray[0], numArray[1]);
        }

        /// <summary>
        /// Converts the given value object to the specified type, using the specified context and culture information.
        /// </summary>
        /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context.</param>
        /// <param name="culture">A <see cref="T:System.Globalization.CultureInfo" />. If null is passed, the current culture is assumed.</param>
        /// <param name="value">The <see cref="T:System.Object" /> to convert.</param>
        /// <param name="destinationType">The <see cref="T:System.Type" /> to convert the <paramref name="value" /> parameter to.</param>
        /// <returns>An <see cref="T:System.Object" /> that represents the converted value.</returns>
        /// <exception cref="System.ArgumentNullException">destinationType</exception>
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == null)
                throw new ArgumentNullException("destinationType");

            if ((destinationType == typeof(string)) && (value is Point))
            {
                var size = (Point)value;
                if (culture == null)
                    culture = CultureInfo.CurrentCulture;

                var separator = "; ";
                var converter = TypeDescriptor.GetConverter(typeof(int));
                var strArray = new string[2];
                var num = 0;
                strArray[num++] = converter.ConvertToString(context, culture, size.X);
                strArray[num++] = converter.ConvertToString(context, culture, size.Y);
                return string.Join(separator, strArray);
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
