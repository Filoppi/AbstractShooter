using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;

namespace AbstractShooter
{
	public interface IShapeF
	{
		float Left { get; }
		float Top { get; }
		float Right { get; }
		float Bottom { get; }

		RectangleF GetBoundingRectangle();

		bool Contains(float x, float y);

		bool Contains(Vector2 point);
	}

	public struct Size : IEquatable<Size>
	{
		public Size(int width, int height)
			: this()
		{
			Width = width;
			Height = height;
		}

		public int Width { get; }
		public int Height { get; }

		public static Size Empty => new Size(0, 0);

		public static Size MaxValue => new Size(int.MaxValue, int.MaxValue);

		public bool IsEmpty => Width == 0 && Height == 0;

		public override int GetHashCode()
		{
			unchecked
			{
				return Width.GetHashCode() + Height.GetHashCode();
			}
		}

		public static bool operator ==(Size a, Size b)
		{
			return a.Width == b.Width && a.Height == b.Height;
		}

		public static bool operator !=(Size a, Size b)
		{
			return !(a == b);
		}

		public bool Equals(Size other)
		{
			return Width == other.Width && Height == other.Height;
		}

		public static implicit operator Point(Size size)
		{
			return new Point(size.Width, size.Height);
		}

		public static implicit operator Vector2(Size size)
		{
			return new Vector2(size.Width, size.Height);
		}

		public static implicit operator SizeF(Size size)
		{
			return new SizeF(size.Width, size.Height);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is Size && Equals((Size)obj);
		}

		public override string ToString()
		{
			return $"Width: {Width}, Height: {Height}";
		}
	}

	public struct SizeF : IEquatable<SizeF>
	{
		public SizeF(float width, float height)
			: this()
		{
			Width = width;
			Height = height;
		}

		public float Width { get; }
		public float Height { get; }
		public static Size Empty => new Size(0, 0);
		public static Size MaxValue => new Size(int.MaxValue, int.MaxValue);
		public bool IsEmpty => Width.Equals(0) && Height.Equals(0);

		public override int GetHashCode()
		{
			unchecked
			{
				return Width.GetHashCode() + Height.GetHashCode();
			}
		}

		public static bool operator ==(SizeF a, SizeF b)
		{
			return a.Width.Equals(b.Width) && a.Height.Equals(b.Height);
		}

		public static bool operator !=(SizeF a, SizeF b)
		{
			return !(a == b);
		}

		public static SizeF operator /(SizeF size, float value)
		{
			return new SizeF(size.Width / value, size.Height / value);
		}

		public static SizeF operator *(SizeF size, float value)
		{
			return new SizeF(size.Width * value, size.Height * value);
		}

		public bool Equals(SizeF other)
		{
			return Width.Equals(other.Width) && Height.Equals(other.Height);
		}

		public static implicit operator SizeF(Point size)
		{
			return new SizeF(size.X, size.Y);
		}

		public static implicit operator Vector2(SizeF size)
		{
			return new Vector2(size.Width, size.Height);
		}

		public static explicit operator Size(SizeF size)
		{
			return new Size((int)size.Width, (int)size.Height);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is Size && Equals((Size)obj);
		}

		public override string ToString()
		{
			return $"Width: {Width}, Height: {Height}";
		}
	}

	/// <summary>
	/// Describes a floating point 2D-rectangle.
	/// </summary>
	[DataContract]
	[DebuggerDisplay("{DebugDisplayString,nq}")]
	public struct RectangleF : IShapeF, IEquatable<RectangleF>
	{
		/// <summary>
		/// The x coordinate of the top-left corner of this <see cref="RectangleF"/>.
		/// </summary>
		[DataMember]
		public float X;

		/// <summary>
		/// The y coordinate of the top-left corner of this <see cref="RectangleF"/>.
		/// </summary>
		[DataMember]
		public float Y;

		/// <summary>
		/// The width of this <see cref="RectangleF"/>.
		/// </summary>
		[DataMember]
		public float Width;

		/// <summary>
		/// The height of this <see cref="RectangleF"/>.
		/// </summary>
		[DataMember]
		public float Height;

		/// <summary>
		/// Returns a <see cref="RectangleF"/> with X=0, Y=0, Width=0, Height=0.
		/// </summary>
		public static RectangleF Empty { get; } = new RectangleF();

		/// <summary>
		/// Returns the x coordinate of the left edge of this <see cref="RectangleF"/>.
		/// </summary>
		public float Left => X;

		/// <summary>
		/// Returns the x coordinate of the right edge of this <see cref="RectangleF"/>.
		/// </summary>
		public float Right => X + Width;

		/// <summary>
		/// Returns the y coordinate of the top edge of this <see cref="RectangleF"/>.
		/// </summary>
		public float Top => Y;

		/// <summary>
		/// Returns the y coordinate of the bottom edge of this <see cref="RectangleF"/>.
		/// </summary>
		public float Bottom => Y + Height;

		/// <summary>
		/// Whether or not this <see cref="RectangleF"/> has a <see cref="Width"/> and
		/// <see cref="Height"/> of 0, and a <see cref="Location"/> of (0, 0).
		/// </summary>
		public bool IsEmpty => Width.Equals(0) && Height.Equals(0) && X.Equals(0) && Y.Equals(0);

		/// <summary>
		/// The top-left coordinates of this <see cref="RectangleF"/>.
		/// </summary>
		public Vector2 Location
		{
			get { return new Vector2(X, Y); }
			set
			{
				X = value.X;
				Y = value.Y;
			}
		}

		/// <summary>
		/// The width-height coordinates of this <see cref="RectangleF"/>.
		/// </summary>
		public SizeF Size
		{
			get { return new SizeF(Width, Height); }
			set
			{
				Width = value.Width;
				Height = value.Height;
			}
		}

		/// <summary>
		/// A <see cref="Vector2"/> located in the center of this <see cref="RectangleF"/>.
		/// </summary>
		public Vector2 Center => new Vector2(X + Width / 2f, Y + Height / 2f);

		internal string DebugDisplayString => string.Concat(X, "  ", Y, "  ", Width, "  ", Height);

		/// <summary>
		/// Creates a new instance of <see cref="RectangleF"/> struct, with the specified
		/// position, width, and height.
		/// </summary>
		/// <param name="x">The x coordinate of the top-left corner of the created <see cref="RectangleF"/>.</param>
		/// <param name="y">The y coordinate of the top-left corner of the created <see cref="RectangleF"/>.</param>
		/// <param name="width">The width of the created <see cref="RectangleF"/>.</param>
		/// <param name="height">The height of the created <see cref="RectangleF"/>.</param>
		public RectangleF(float x, float y, float width, float height)
		{
			X = x;
			Y = y;
			Width = width;
			Height = height;
		}

		/// <summary>
		/// Creates a new instance of <see cref="RectangleF"/> struct, with the specified
		/// location and size.
		/// </summary>
		/// <param name="location">The x and y coordinates of the top-left corner of the created <see cref="RectangleF"/>.</param>
		/// <param name="size">The width and height of the created <see cref="RectangleF"/>.</param>
		public RectangleF(Vector2 location, SizeF size)
		{
			X = location.X;
			Y = location.Y;
			Width = size.Width;
			Height = size.Height;
		}

		/// <summary>
		/// Creates a new instance of <see cref="RectangleF"/> struct, based on a <see cref="Rectangle"/>
		/// </summary>
		/// <param name="rect">The source <see cref="Rectangle"/>.</param>
		public RectangleF(Rectangle rect)
		{
			X = rect.X;
			Y = rect.Y;
			Width = rect.Width;
			Height = rect.Height;
		}

		/// <summary>
		/// Allow implict cast from a <see cref="Rectangle"/>
		/// </summary>
		/// <param name="rect">The <see cref="Rectangle"/> to be cast.</param>
		public static implicit operator RectangleF(Rectangle rect)
		{
			return new RectangleF(rect);
		}

		/// <summary>
		/// Allow implict cast from a <see cref="Rectangle"/>
		/// </summary>
		/// <param name="rect">The <see cref="Rectangle"/> to be cast.</param>
		public static implicit operator RectangleF? (Rectangle? rect)
		{
			if (!rect.HasValue)
				return null;

			return new RectangleF(rect.Value);
		}

		/// <summary>
		/// Allow explict cast to a <see cref="Rectangle"/>
		/// </summary>
		/// <remark>
		/// Loss of precision due to the truncation from <see cref="float"/> to <see cref="int"/>.
		/// </remark>
		/// <param name="rect">The <see cref="RectangleF"/> to be cast.</param>
		public static explicit operator Rectangle(RectangleF rect)
		{
			return new Rectangle((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);
		}

		/// <summary>
		/// Compares whether two <see cref="RectangleF"/> instances are equal.
		/// </summary>
		/// <param name="a"><see cref="RectangleF"/> instance on the left of the equal sign.</param>
		/// <param name="b"><see cref="RectangleF"/> instance on the right of the equal sign.</param>
		/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
		public static bool operator ==(RectangleF a, RectangleF b)
		{
			const float epsilon = 0.00001f;
			return Math.Abs(a.X - b.X) < epsilon
				&& Math.Abs(a.Y - b.Y) < epsilon
				&& Math.Abs(a.Width - b.Width) < epsilon
				&& Math.Abs(a.Height - b.Height) < epsilon;
		}

		/// <summary>
		/// Compares whether two <see cref="RectangleF"/> instances are not equal.
		/// </summary>
		/// <param name="a"><see cref="RectangleF"/> instance on the left of the not equal sign.</param>
		/// <param name="b"><see cref="RectangleF"/> instance on the right of the not equal sign.</param>
		/// <returns><c>true</c> if the instances are not equal; <c>false</c> otherwise.</returns>
		public static bool operator !=(RectangleF a, RectangleF b)
		{
			return !(a == b);
		}

		/// <summary>
		/// Gets whether or not the provided coordinates lie within the bounds of this <see cref="RectangleF"/>.
		/// </summary>
		/// <param name="x">The x coordinate of the point to check for containment.</param>
		/// <param name="y">The y coordinate of the point to check for containment.</param>
		/// <returns><c>true</c> if the provided coordinates lie inside this <see cref="RectangleF"/>; <c>false</c> otherwise.</returns>
		public bool Contains(int x, int y)
		{
			return X <= x && x < X + Width && Y <= y && y < Y + Height;
		}

		public RectangleF GetBoundingRectangle()
		{
			return this;
		}

		/// <summary>
		/// Gets whether or not the provided coordinates lie within the bounds of this <see cref="RectangleF"/>.
		/// </summary>
		/// <param name="x">The x coordinate of the point to check for containment.</param>
		/// <param name="y">The y coordinate of the point to check for containment.</param>
		/// <returns><c>true</c> if the provided coordinates lie inside this <see cref="RectangleF"/>; <c>false</c> otherwise.</returns>
		public bool Contains(float x, float y)
		{
			return X <= x && x < X + Width && Y <= y && y < Y + Height;
		}

		/// <summary>
		/// Gets whether or not the provided <see cref="Point"/> lies within the bounds of this <see cref="RectangleF"/>.
		/// </summary>
		/// <param name="value">The coordinates to check for inclusion in this <see cref="RectangleF"/>.</param>
		/// <returns><c>true</c> if the provided <see cref="Point"/> lies inside this <see cref="RectangleF"/>; <c>false</c> otherwise.</returns>
		public bool Contains(Point value)
		{
			return X <= value.X && value.X < X + Width && Y <= value.Y && value.Y < Y + Height;
		}

		/// <summary>
		/// Gets whether or not the provided <see cref="Point"/> lies within the bounds of this <see cref="RectangleF"/>.
		/// </summary>
		/// <param name="value">The coordinates to check for inclusion in this <see cref="RectangleF"/>.</param>
		/// <param name="result"><c>true</c> if the provided <see cref="Point"/> lies inside this <see cref="RectangleF"/>; <c>false</c> otherwise. As an output parameter.</param>
		public void Contains(ref Point value, out bool result)
		{
			result = X <= value.X && value.X < X + Width && Y <= value.Y && value.Y < Y + Height;
		}

		/// <summary>
		/// Gets whether or not the provided <see cref="Vector2"/> lies within the bounds of this <see cref="RectangleF"/>.
		/// </summary>
		/// <param name="value">The coordinates to check for inclusion in this <see cref="RectangleF"/>.</param>
		/// <returns><c>true</c> if the provided <see cref="Vector2"/> lies inside this <see cref="RectangleF"/>; <c>false</c> otherwise.</returns>
		public bool Contains(Vector2 value)
		{
			return X <= value.X && value.X < X + Width && Y <= value.Y && value.Y < Y + Height;
		}

		/// <summary>
		/// Gets whether or not the provided <see cref="Vector2"/> lies within the bounds of this <see cref="RectangleF"/>.
		/// </summary>
		/// <param name="value">The coordinates to check for inclusion in this <see cref="RectangleF"/>.</param>
		/// <param name="result"><c>true</c> if the provided <see cref="Vector2"/> lies inside this <see cref="RectangleF"/>; <c>false</c> otherwise. As an output parameter.</param>
		public void Contains(ref Vector2 value, out bool result)
		{
			result = (X <= value.X) && (value.X < X + Width) && (Y <= value.Y) && (value.Y < Y + Height);
		}

		/// <summary>
		/// Gets whether or not the provided <see cref="RectangleF"/> lies within the bounds of this <see cref="RectangleF"/>.
		/// </summary>
		/// <param name="value">The <see cref="RectangleF"/> to check for inclusion in this <see cref="RectangleF"/>.</param>
		/// <returns><c>true</c> if the provided <see cref="RectangleF"/>'s bounds lie entirely inside this <see cref="RectangleF"/>; <c>false</c> otherwise.</returns>
		public bool Contains(RectangleF value)
		{
			return (X <= value.X) && (value.X + value.Width <= X + Width) && (Y <= value.Y) && (value.Y + value.Height <= Y + Height);
		}

		/// <summary>
		/// Gets whether or not the provided <see cref="RectangleF"/> lies within the bounds of this <see cref="RectangleF"/>.
		/// </summary>
		/// <param name="value">The <see cref="RectangleF"/> to check for inclusion in this <see cref="RectangleF"/>.</param>
		/// <param name="result"><c>true</c> if the provided <see cref="RectangleF"/>'s bounds lie entirely inside this <see cref="RectangleF"/>; <c>false</c> otherwise. As an output parameter.</param>
		public void Contains(ref RectangleF value, out bool result)
		{
			result = (X <= value.X) && (value.X + value.Width <= X + Width) && (Y <= value.Y) && (value.Y + value.Height <= Y + Height);
		}

		/// <summary>
		/// Compares whether current instance is equal to specified <see cref="Object"/>.
		/// </summary>
		/// <param name="obj">The <see cref="Object"/> to compare.</param>
		/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
		public override bool Equals(object obj)
		{
			return obj is RectangleF && this == (RectangleF)obj;
		}

		/// <summary>
		/// Compares whether current instance is equal to specified <see cref="RectangleF"/>.
		/// </summary>
		/// <param name="other">The <see cref="RectangleF"/> to compare.</param>
		/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
		public bool Equals(RectangleF other)
		{
			return this == other;
		}

		/// <summary>
		/// Gets the hash code of this <see cref="RectangleF"/>.
		/// </summary>
		/// <returns>Hash code of this <see cref="RectangleF"/>.</returns>
		public override int GetHashCode()
		{
			// ReSharper disable NonReadonlyMemberInGetHashCode
			return X.GetHashCode() ^ Y.GetHashCode() ^ Width.GetHashCode() ^ Height.GetHashCode();
			// ReSharper restore NonReadonlyMemberInGetHashCode
		}

		/// <summary>
		/// Adjusts the edges of this <see cref="RectangleF"/> by specified horizontal and vertical amounts.
		/// </summary>
		/// <param name="horizontalAmount">Value to adjust the left and right edges.</param>
		/// <param name="verticalAmount">Value to adjust the top and bottom edges.</param>
		public void Inflate(int horizontalAmount, int verticalAmount)
		{
			X -= horizontalAmount;
			Y -= verticalAmount;
			Width += horizontalAmount * 2;
			Height += verticalAmount * 2;
		}

		/// <summary>
		/// Adjusts the edges of this <see cref="RectangleF"/> by specified horizontal and vertical amounts.
		/// </summary>
		/// <param name="horizontalAmount">Value to adjust the left and right edges.</param>
		/// <param name="verticalAmount">Value to adjust the top and bottom edges.</param>
		public void Inflate(float horizontalAmount, float verticalAmount)
		{
			X -= horizontalAmount;
			Y -= verticalAmount;
			Width += horizontalAmount * 2;
			Height += verticalAmount * 2;
		}

		/// <summary>
		/// Gets whether or not the other <see cref="RectangleF"/> intersects with this RectangleF.
		/// </summary>
		/// <param name="value">The other rectangle for testing.</param>
		/// <returns><c>true</c> if other <see cref="RectangleF"/> intersects with this rectangle; <c>false</c> otherwise.</returns>
		public bool Intersects(RectangleF value)
		{
			return value.Left < Right && Left < value.Right &&
				   value.Top < Bottom && Top < value.Bottom;
		}

		/// <summary>
		/// Gets whether or not the other <see cref="RectangleF"/> intersects with this rectangle.
		/// </summary>
		/// <param name="value">The other rectangle for testing.</param>
		/// <param name="result"><c>true</c> if other <see cref="RectangleF"/> intersects with this rectangle; <c>false</c> otherwise. As an output parameter.</param>
		public void Intersects(ref RectangleF value, out bool result)
		{
			result = value.Left < Right && Left < value.Right &&
					 value.Top < Bottom && Top < value.Bottom;
		}

		/// <summary>
		/// Creates a new <see cref="RectangleF"/> that contains overlapping region of two other rectangles.
		/// </summary>
		/// <param name="value1">The first <see cref="RectangleF"/>.</param>
		/// <param name="value2">The second <see cref="RectangleF"/>.</param>
		/// <returns>Overlapping region of the two rectangles.</returns>
		public static RectangleF Intersect(RectangleF value1, RectangleF value2)
		{
			RectangleF rectangle;
			Intersect(ref value1, ref value2, out rectangle);
			return rectangle;
		}

		/// <summary>
		/// Creates a new <see cref="RectangleF"/> that contains overlapping region of two other rectangles.
		/// </summary>
		/// <param name="value1">The first <see cref="RectangleF"/>.</param>
		/// <param name="value2">The second <see cref="RectangleF"/>.</param>
		/// <param name="result">Overlapping region of the two rectangles as an output parameter.</param>
		public static void Intersect(ref RectangleF value1, ref RectangleF value2, out RectangleF result)
		{
			if (value1.Intersects(value2))
			{
				var rightSide = Math.Min(value1.X + value1.Width, value2.X + value2.Width);
				var leftSide = Math.Max(value1.X, value2.X);
				var topSide = Math.Max(value1.Y, value2.Y);
				var bottomSide = Math.Min(value1.Y + value1.Height, value2.Y + value2.Height);
				result = new RectangleF(leftSide, topSide, rightSide - leftSide, bottomSide - topSide);
			}
			else
			{
				result = new RectangleF(0, 0, 0, 0);
			}
		}

		/// <summary>
		/// Changes the <see cref="Location"/> of this <see cref="RectangleF"/>.
		/// </summary>
		/// <param name="offsetX">The x coordinate to add to this <see cref="RectangleF"/>.</param>
		/// <param name="offsetY">The y coordinate to add to this <see cref="RectangleF"/>.</param>
		public void Offset(int offsetX, int offsetY)
		{
			X += offsetX;
			Y += offsetY;
		}

		/// <summary>
		/// Changes the <see cref="Location"/> of this <see cref="RectangleF"/>.
		/// </summary>
		/// <param name="offsetX">The x coordinate to add to this <see cref="RectangleF"/>.</param>
		/// <param name="offsetY">The y coordinate to add to this <see cref="RectangleF"/>.</param>
		public void Offset(float offsetX, float offsetY)
		{
			X += offsetX;
			Y += offsetY;
		}

		/// <summary>
		/// Changes the <see cref="Location"/> of this <see cref="RectangleF"/>.
		/// </summary>
		/// <param name="amount">The x and y components to add to this <see cref="RectangleF"/>.</param>
		public void Offset(Point amount)
		{
			X += amount.X;
			Y += amount.Y;
		}

		/// <summary>
		/// Changes the <see cref="Location"/> of this <see cref="RectangleF"/>.
		/// </summary>
		/// <param name="amount">The x and y components to add to this <see cref="RectangleF"/>.</param>
		public void Offset(Vector2 amount)
		{
			X += amount.X;
			Y += amount.Y;
		}

		/// <summary>
		/// Returns a <see cref="String"/> representation of this <see cref="RectangleF"/> in the format:
		/// {X:[<see cref="X"/>] Y:[<see cref="Y"/>] Width:[<see cref="Width"/>] Height:[<see cref="Height"/>]}
		/// </summary>
		/// <returns><see cref="String"/> representation of this <see cref="RectangleF"/>.</returns>
		public override string ToString()
		{
			return "{X:" + X + " Y:" + Y + " Width:" + Width + " Height:" + Height + "}";
		}

		/// <summary>
		/// Creates a new <see cref="RectangleF"/> that completely contains two other rectangles.
		/// </summary>
		/// <param name="value1">The first <see cref="RectangleF"/>.</param>
		/// <param name="value2">The second <see cref="RectangleF"/>.</param>
		/// <returns>The union of the two rectangles.</returns>
		public static RectangleF Union(RectangleF value1, RectangleF value2)
		{
			var x = Math.Min(value1.X, value2.X);
			var y = Math.Min(value1.Y, value2.Y);
			return new RectangleF(x, y,
				Math.Max(value1.Right, value2.Right) - x,
				Math.Max(value1.Bottom, value2.Bottom) - y);
		}

		/// <summary>
		/// Creates a new <see cref="RectangleF"/> that completely contains two other rectangles.
		/// </summary>
		/// <param name="value1">The first <see cref="RectangleF"/>.</param>
		/// <param name="value2">The second <see cref="RectangleF"/>.</param>
		/// <param name="result">The union of the two rectangles as an output parameter.</param>
		public static void Union(ref RectangleF value1, ref RectangleF value2, out RectangleF result)
		{
			result.X = Math.Min(value1.X, value2.X);
			result.Y = Math.Min(value1.Y, value2.Y);
			result.Width = Math.Max(value1.Right, value2.Right) - result.X;
			result.Height = Math.Max(value1.Bottom, value2.Bottom) - result.Y;
		}

		/// <summary>
		/// Creates a new <see cref="RectangleF"/> from two points.
		/// </summary>
		/// <param name="point0">The top left or bottom right corner</param>
		/// <param name="point1">The bottom left or top right corner</param>
		/// <returns></returns>
		public static RectangleF FromPoints(Vector2 point0, Vector2 point1)
		{
			var x = Math.Min(point0.X, point1.X);
			var y = Math.Min(point0.Y, point1.Y);
			var width = Math.Abs(point0.X - point1.X);
			var height = Math.Abs(point0.Y - point1.Y);
			var rectangle = new RectangleF(x, y, width, height);
			return rectangle;
		}

		public Rectangle ToRectangle()
		{
			return new Rectangle((int)X, (int)Y, (int)Width, (int)Height);
		}

		public RectangleF ToRectangleF()
		{
			return new RectangleF(X, Y, Width, Height);
		}
	}

	/// <summary>
	/// Describes a 2D-circle.
	/// </summary>
	[DataContract]
	[DebuggerDisplay("{DebugDisplayString,nq}")]
	public struct CircleF : IShapeF, IEquatable<CircleF>
	{
		private static readonly CircleF _empty = new CircleF();

		/// <summary>
		/// The point representing the center of this <see cref="CircleF"/>.
		/// </summary>
		[DataMember]
		public Vector2 Center { get; set; }

		/// <summary>
		/// The radius from the center of this <see cref="CircleF"/>.
		/// </summary>
		[DataMember]
		public float Radius { get; set; }

		/// <summary>
		/// Returns a <see cref="CircleF"/> with Point = Vector2.Zero and Radius= 0.
		/// </summary>
		public static CircleF Empty => _empty;

		/// <summary>
		/// Returns the x coordinate of the far left point of this <see cref="CircleF"/>.
		/// </summary>
		public float Left => Center.X - Radius;

		/// <summary>
		/// Returns the x coordinate of the far right point of this <see cref="CircleF"/>.
		/// </summary>
		public float Right => Center.X + Radius;

		/// <summary>
		/// Returns the y coordinate of the far top point of this <see cref="CircleF"/>.
		/// </summary>
		public float Top => Center.Y - Radius;

		/// <summary>
		/// Returns the y coordinate of the far bottom point of this <see cref="CircleF"/>.
		/// </summary>
		public float Bottom => Center.Y + Radius;

		/// <summary>
		/// The center coordinates of this <see cref="CircleF"/>.
		/// </summary>
		public Point Location
		{
			get { return Center.ToPoint(); }
			set { Center = value.ToVector2(); }
		}

		/// <summary>
		/// Returns the diameter of this <see cref="CircleF"/>
		/// </summary>
		public float Diameter => Radius * 2.0f;

		/// <summary>
		/// Returns the Circumference of this <see cref="CircleF"/>
		/// </summary>
		public float Circumference => 2.0f * MathHelper.Pi * Radius;

		/// <summary>
		/// Whether or not this <see cref="CircleF"/> has a <see cref="Center"/> and
		/// <see cref="Radius"/> of 0.
		/// </summary>
		public bool IsEmpty => Radius.Equals(0) && (Center == Vector2.Zero);

		internal string DebugDisplayString => $"{Center} {Radius}";

		/// <summary>
		/// Creates a new instance of <see cref="CircleF"/> struct, with the specified
		/// position, and radius
		/// </summary>
		/// <param name="center">The position of the center of the created <see cref="CircleF"/>.</param>
		/// <param name="radius">The radius of the created <see cref="CircleF"/>.</param>
		public CircleF(Vector2 center, float radius)
			: this()
		{
			Center = center;
			Radius = radius;
		}

		/// <summary>
		/// Compares whether two <see cref="CircleF"/> instances are equal.
		/// </summary>
		/// <param name="a"><see cref="CircleF"/> instance on the left of the equal sign.</param>
		/// <param name="b"><see cref="CircleF"/> instance on the right of the equal sign.</param>
		/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
		public static bool operator ==(CircleF a, CircleF b)
		{
			return a.Center == b.Center && a.Radius.Equals(b.Radius);
		}

		/// <summary>
		/// Compares whether two <see cref="CircleF"/> instances are not equal.
		/// </summary>
		/// <param name="a"><see cref="CircleF"/> instance on the left of the not equal sign.</param>
		/// <param name="b"><see cref="CircleF"/> instance on the right of the not equal sign.</param>
		/// <returns><c>true</c> if the instances are not equal; <c>false</c> otherwise.</returns>
		public static bool operator !=(CircleF a, CircleF b)
		{
			return !(a == b);
		}

		/// <summary>
		/// Gets the point at the edge of this <see cref="CircleF"/> from the provided angle
		/// </summary>
		/// <param name="angle">an angle in radians</param>
		/// <returns><see cref="Vector2"/> representing the point on this <see cref="CircleF"/>'s surface at the specified angle</returns>
		public Vector2 GetPointAlongEdge(float angle)
		{
			return new Vector2(Center.X + Radius * (float)Math.Cos(angle),
							   Center.Y + Radius * (float)Math.Sin(angle));
		}

		public RectangleF GetBoundingRectangle()
		{
			var minX = Left;
			var minY = Top;
			var maxX = Right;
			var maxY = Bottom;

			return new RectangleF(minX, minY, maxX - minX, maxY - minY);
		}

		/// <summary>
		/// Gets whether or not the provided coordinates lie within the bounds of this <see cref="CircleF"/>.
		/// </summary>
		/// <param name="x">The x coordinate of the point to check for containment.</param>
		/// <param name="y">The y coordinate of the point to check for containment.</param>
		/// <returns><c>true</c> if the provided coordinates lie inside this <see cref="CircleF"/>; <c>false</c> otherwise.</returns>
		public bool Contains(float x, float y)
		{
			return (new Vector2(x, y) - Center).LengthSquared() <= Radius * Radius;
		}

		/// <summary>
		/// Gets whether or not the provided <see cref="Point"/> lies within the bounds of this <see cref="CircleF"/>.
		/// </summary>
		/// <param name="value">The coordinates to check for inclusion in this <see cref="CircleF"/>.</param>
		/// <returns><c>true</c> if the provided <see cref="Point"/> lies inside this <see cref="CircleF"/>; <c>false</c> otherwise.</returns>
		public bool Contains(Point value)
		{
			return (value.ToVector2() - Center).LengthSquared() <= Radius * Radius;
		}

		/// <summary>
		/// Gets whether or not the provided <see cref="Point"/> lies within the bounds of this <see cref="CircleF"/>.
		/// </summary>
		/// <param name="value">The coordinates to check for inclusion in this <see cref="CircleF"/>.</param>
		/// <param name="result"><c>true</c> if the provided <see cref="Point"/> lies inside this <see cref="CircleF"/>; <c>false</c> otherwise. As an output parameter.</param>
		public void Contains(ref Point value, out bool result)
		{
			result = (value.ToVector2() - Center).LengthSquared() <= Radius * Radius;
		}

		/// <summary>
		/// Gets whether or not the provided <see cref="Vector2"/> lies within the bounds of this <see cref="CircleF"/>.
		/// </summary>
		/// <param name="value">The coordinates to check for inclusion in this <see cref="CircleF"/>.</param>
		/// <returns><c>true</c> if the provided <see cref="Vector2"/> lies inside this <see cref="CircleF"/>; <c>false</c> otherwise.</returns>
		public bool Contains(Vector2 value)
		{
			return (value - Center).LengthSquared() <= Radius * Radius;
		}

		/// <summary>
		/// Gets whether or not the provided <see cref="Vector2"/> lies within the bounds of this <see cref="CircleF"/>.
		/// </summary>
		/// <param name="value">The coordinates to check for inclusion in this <see cref="CircleF"/>.</param>
		/// <param name="result"><c>true</c> if the provided <see cref="Vector2"/> lies inside this <see cref="CircleF"/>; <c>false</c> otherwise. As an output parameter.</param>
		public void Contains(ref Vector2 value, out bool result)
		{
			result = (value - Center).LengthSquared() <= Radius * Radius;
		}

		/// <summary>
		/// Gets whether or not the provided <see cref="CircleF"/> lies within the bounds of this <see cref="CircleF"/>.
		/// </summary>
		/// <param name="value">The <see cref="CircleF"/> to check for inclusion in this <see cref="CircleF"/>.</param>
		/// <returns><c>true</c> if the provided <see cref="CircleF"/>'s center lie entirely inside this <see cref="CircleF"/>; <c>false</c> otherwise.</returns>
		public bool Contains(CircleF value)
		{
			var distanceOfCenter = value.Center - Center;
			var radii = Radius - value.Radius;

			return distanceOfCenter.X * distanceOfCenter.X + distanceOfCenter.Y * distanceOfCenter.Y <= Math.Abs(radii * radii);
		}

		/// <summary>
		/// Gets whether or not the provided <see cref="CircleF"/> lies within the bounds of this <see cref="CircleF"/>.
		/// </summary>
		/// <param name="value">The <see cref="CircleF"/> to check for inclusion in this <see cref="CircleF"/>.</param>
		/// <param name="result"><c>true</c> if the provided <see cref="CircleF"/>'s center lie entirely inside this <see cref="CircleF"/>; <c>false</c> otherwise. As an output parameter.</param>
		public void Contains(ref CircleF value, out bool result)
		{
			var distanceOfCenter = value.Center - Center;
			var radii = Radius - value.Radius;

			result = distanceOfCenter.X * distanceOfCenter.X + distanceOfCenter.Y * distanceOfCenter.Y <= Math.Abs(radii * radii);
		}

		/// <summary>
		/// Compares whether current instance is equal to specified <see cref="Object"/>.
		/// </summary>
		/// <param name="obj">The <see cref="Object"/> to compare.</param>
		/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
		public override bool Equals(object obj)
		{
			return obj is CircleF && this == (CircleF)obj;
		}

		/// <summary>
		/// Compares whether current instance is equal to specified <see cref="CircleF"/>.
		/// </summary>
		/// <param name="other">The <see cref="CircleF"/> to compare.</param>
		/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
		public bool Equals(CircleF other)
		{
			return this == other;
		}

		/// <summary>
		/// Gets the hash code of this <see cref="CircleF"/>.
		/// </summary>
		/// <returns>Hash code of this <see cref="CircleF"/>.</returns>
		public override int GetHashCode()
		{
			// ReSharper disable NonReadonlyMemberInGetHashCode
			return Center.GetHashCode() ^ Radius.GetHashCode();
			// ReSharper restore NonReadonlyMemberInGetHashCode
		}

		/// <summary>
		/// Adjusts the size of this <see cref="CircleF"/> by specified radius amount.
		/// </summary>
		/// <param name="radiusAmount">Value to adjust the radius by.</param>
		public void Inflate(float radiusAmount)
		{
			Center -= new Vector2(radiusAmount);
			Radius += radiusAmount * 2;
		}

		/// <summary>
		/// Gets whether or not a specified <see cref="CircleF"/> intersects with this <see cref="CircleF"/>.
		/// </summary>
		/// <param name="value">Other <see cref="CircleF"/>.</param>
		/// <returns><c>true</c> if other <see cref="CircleF"/> intersects with this <see cref="CircleF"/>; <c>false</c> otherwise.</returns>
		public bool Intersects(CircleF value)
		{
			var distanceOfCenter = value.Center - Center;
			var radii = Radius + value.Radius;

			return distanceOfCenter.X * distanceOfCenter.X + distanceOfCenter.Y * distanceOfCenter.Y < radii * radii;
		}

		/// <summary>
		/// Gets whether or not a specified <see cref="CircleF"/> intersects with this <see cref="CircleF"/>.
		/// </summary>
		/// <param name="value">Other <see cref="CircleF"/>.</param>
		/// <param name="result"><c>true</c> if other <see cref="CircleF"/> intersects with this <see cref="CircleF"/>; <c>false</c> otherwise. As an output parameter.</param>
		public void Intersects(ref CircleF value, out bool result)
		{
			var distanceOfCenter = value.Center - Center;
			var radii = Radius + value.Radius;

			result = distanceOfCenter.X * distanceOfCenter.X + distanceOfCenter.Y * distanceOfCenter.Y < radii * radii;
		}

		/// <summary>
		/// Gets whether or not a specified <see cref="Rectangle"/> intersects with this <see cref="CircleF"/>.
		/// </summary>
		/// <param name="value">Other <see cref="Rectangle"/>.</param>
		/// <returns><c>true</c> if other <see cref="Rectangle"/> intersects with this <see cref="CircleF"/>; <c>false</c> otherwise.</returns>
		public bool Intersects(Rectangle value)
		{
			var distance = new Vector2(Math.Abs(Center.X - value.X), Math.Abs(Center.Y - value.Y));

			if (distance.X > value.Width / 2.0f + Radius)
				return false;

			if (distance.Y > value.Height / 2.0f + Radius)
				return false;

			if (distance.X <= value.Width / 2.0f)
				return true;

			if (distance.Y <= value.Height / 2.0f)
				return true;

			var distanceOfCorners =
				(distance.X - value.Width / 2.0f) *
				(distance.X - value.Width / 2.0f) +
				(distance.Y - value.Height / 2.0f) *
				(distance.Y - value.Height / 2.0f);

			return distanceOfCorners <= Radius * Radius;
		}

		/// <summary>
		/// Gets whether or not a specified <see cref="Rectangle"/> intersects with this <see cref="CircleF"/>.
		/// </summary>
		/// <param name="value">Other <see cref="Rectangle"/>.</param>
		/// <param name="result"><c>true</c> if other <see cref="Rectangle"/> intersects with this <see cref="CircleF"/>; <c>false</c> otherwise. As an output parameter.</param>
		public void Intersects(ref Rectangle value, out bool result)
		{
			result = Intersects(value);
		}

		/// <summary>
		/// Changes the <see cref="Location"/> of this <see cref="CircleF"/>.
		/// </summary>
		/// <param name="offsetX">The x coordinate to add to this <see cref="CircleF"/>.</param>
		/// <param name="offsetY">The y coordinate to add to this <see cref="CircleF"/>.</param>
		public void Offset(float offsetX, float offsetY)
		{
			Offset(new Vector2(offsetX, offsetY));
		}

		/// <summary>
		/// Changes the <see cref="Location"/> of this <see cref="CircleF"/>.
		/// </summary>
		/// <param name="amount">The x and y components to add to this <see cref="CircleF"/>.</param>
		public void Offset(Point amount)
		{
			Offset(amount.ToVector2());
		}

		/// <summary>
		/// Changes the <see cref="Location"/> of this <see cref="CircleF"/>.
		/// </summary>
		/// <param name="amount">The x and y components to add to this <see cref="CircleF"/>.</param>
		public void Offset(Vector2 amount)
		{
			Center += new Vector2(amount.X, amount.Y);
		}

		/// <summary>
		/// Returns a <see cref="String"/> representation of this <see cref="CircleF"/> in the format:
		/// {Center:[<see cref="Center"/>] Radius:[<see cref="Radius"/>]}
		/// </summary>
		/// <returns><see cref="String"/> representation of this <see cref="CircleF"/>.</returns>
		public override string ToString()
		{
			return $"{{Center:{Center} Radius:{Radius}}}";
		}

		/// <summary>
		/// Creates a <see cref="Rectangle"/> large enough to fit this <see cref="CircleF"/>
		/// </summary>
		/// <returns><see cref="Rectangle"/> which contains this <see cref="CircleF"/></returns>
		public Rectangle ToRectangle()
		{
			return new Rectangle((int)(Center.X - Radius), (int)(Center.Y - Radius), (int)Radius * 2, (int)Radius * 2);
		}
	}

	public struct PolygonF : IShapeF, IEquatable<PolygonF>
	{
		public PolygonF(IEnumerable<Vector2> vertices)
		{
			_localVertices = vertices.ToArray();
			_transformedVertices = _localVertices;
			_offset = Vector2.Zero;
			_rotation = 0;
			_scale = Vector2.One;
			_isDirty = false;
		}

		private readonly Vector2[] _localVertices;
		private Vector2[] _transformedVertices;
		private Vector2 _offset;
		private float _rotation;
		private Vector2 _scale;
		private bool _isDirty;

		public Vector2[] Vertices
		{
			get
			{
				if (_isDirty)
				{
					_transformedVertices = GetTransformedVertices();
					_isDirty = false;
				}

				return _transformedVertices;
			}
		}

		public float Left { get { return Vertices.Min(v => v.X); } }
		public float Right { get { return Vertices.Max(v => v.X); } }
		public float Top { get { return Vertices.Min(v => v.Y); } }
		public float Bottom { get { return Vertices.Max(v => v.Y); } }

		public void Offset(Vector2 amount)
		{
			_offset += amount;
			_isDirty = true;
		}

		public void Rotate(float amount)
		{
			_rotation += amount;
			_isDirty = true;
		}

		public void Scale(Vector2 amount)
		{
			_scale += amount;
			_isDirty = true;
		}

		private Vector2[] GetTransformedVertices()
		{
			var newVertices = new Vector2[_localVertices.Length];
			var isScaled = _scale != Vector2.One;

			for (var i = 0; i < _localVertices.Length; i++)
			{
				var p = _localVertices[i];

				if (isScaled)
					p *= _scale;

				// ReSharper disable once CompareOfFloatsByEqualityOperator
				if (_rotation != 0)
				{
					var cos = (float)Math.Cos(_rotation);
					var sin = (float)Math.Sin(_rotation);
					p = new Vector2(cos * p.X - sin * p.Y, sin * p.X + cos * p.Y);
				}

				newVertices[i] = p + _offset;
			}

			return newVertices;
		}

		public PolygonF TransformedCopy(Vector2 offset, float rotation, Vector2 scale)
		{
			var polygon = new PolygonF(_localVertices);
			polygon.Offset(offset);
			polygon.Rotate(rotation);
			polygon.Scale(scale - Vector2.One);
			return new PolygonF(polygon.Vertices);
		}

		public RectangleF GetBoundingRectangle()
		{
			var minX = Left;
			var minY = Top;
			var maxX = Right;
			var maxY = Bottom;

			return new RectangleF(minX, minY, maxX - minX, maxY - minY);
		}

		public bool Contains(Vector2 point)
		{
			return Contains(point.X, point.Y);
		}

		public bool Contains(float x, float y)
		{
			var intersects = 0;
			var vertices = Vertices;

			for (var i = 0; i < vertices.Length; i++)
			{
				var x1 = vertices[i].X;
				var y1 = vertices[i].Y;
				var x2 = vertices[(i + 1) % vertices.Length].X;
				var y2 = vertices[(i + 1) % vertices.Length].Y;

				if ((y1 <= y && y < y2 || y2 <= y && y < y1) && x < (x2 - x1) / (y2 - y1) * (y - y1) + x1)
					intersects++;
			}

			return (intersects & 1) == 1;
		}

		public static bool operator ==(PolygonF a, PolygonF b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(PolygonF a, PolygonF b)
		{
			return !(a == b);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is PolygonF && Equals((PolygonF)obj);
		}

		public bool Equals(PolygonF other)
		{
			return Vertices.SequenceEqual(other.Vertices);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return Vertices.Aggregate(27, (current, v) => current + 13 * current + v.GetHashCode());
			}
		}
	}

	public static class SpriteBatchExtensions
	{
		private static Texture2D _texture;

		private static Texture2D GetTexture(SpriteBatch spriteBatch)
		{
			if (_texture == null)
			{
				_texture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
				_texture.SetData(new[] { Color.White });
			}

			return _texture;
		}

		/// <summary>
		/// Draws a closed polygon from a <see cref="PolygonF"/> shape
		/// </summary>
		/// <param name="spriteBatch">The destination drawing surface</param>
		/// /// <param name="position">Where to position the polygon</param>
		/// <param name="polygon">The polygon to draw</param>
		/// <param name="color">The color to use</param>
		/// <param name="thickness">The thickness of the lines</param>
		public static void DrawPolygon(this SpriteBatch spriteBatch, Vector2 position, PolygonF polygon, Color color, float thickness = 1f)
		{
			DrawPolygon(spriteBatch, position, polygon.Vertices, color, thickness);
		}

		/// <summary>
		/// Draws a closed polygon from an array of points
		/// </summary>
		/// <param name="spriteBatch">The destination drawing surface</param>
		/// /// <param name="offset">Where to offset the points</param>
		/// <param name="points">The points to connect with lines</param>
		/// <param name="color">The color to use</param>
		/// <param name="thickness">The thickness of the lines</param>
		public static void DrawPolygon(this SpriteBatch spriteBatch, Vector2 offset, Vector2[] points, Color color, float thickness = 1f)
		{
			if (points.Length == 0)
				return;

			if (points.Length == 1)
			{
				DrawPoint(spriteBatch, points[0], color, (int)thickness);
				return;
			}

			var texture = GetTexture(spriteBatch);

			for (var i = 0; i < points.Length - 1; i++)
				DrawPolygonEdge(spriteBatch, texture, points[i] + offset, points[i + 1] + offset, color, thickness);

			DrawPolygonEdge(spriteBatch, texture, points[points.Length - 1] + offset, points[0] + offset, color, thickness);
		}

		private static void DrawPolygonEdge(SpriteBatch spriteBatch, Texture2D texture, Vector2 point1, Vector2 point2, Color color, float thickness)
		{
			var length = Vector2.Distance(point1, point2);
			var angle = (float)Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);
			var scale = new Vector2(length, thickness);
			spriteBatch.Draw(texture, point1, color: color, rotation: angle, scale: scale);
		}

		/// <summary>
		/// Draws a filled rectangle
		/// </summary>
		/// <param name="spriteBatch">The destination drawing surface</param>
		/// <param name="rectangle">The rectangle to draw</param>
		/// <param name="color">The color to draw the rectangle in</param>
		/// <param name="angle">The angle in radians to draw the rectangle at</param>
		public static void FillRectangle(this SpriteBatch spriteBatch, RectangleF rectangle, Color color)
		{
			FillRectangle(spriteBatch, rectangle.Location, rectangle.Size, color);
		}

		/// <summary>
		/// Draws a filled rectangle
		/// </summary>
		/// <param name="spriteBatch">The destination drawing surface</param>
		/// <param name="location">Where to draw</param>
		/// <param name="size">The size of the rectangle</param>
		/// <param name="angle">The angle in radians to draw the rectangle at</param>
		/// <param name="color">The color to draw the rectangle in</param>
		public static void FillRectangle(this SpriteBatch spriteBatch, Vector2 location, SizeF size, Color color)
		{
			spriteBatch.Draw(GetTexture(spriteBatch), location, null, color, 0, Vector2.Zero, size, SpriteEffects.None, 0);
		}

		/// <summary>
		/// Draws a filled rectangle
		/// </summary>
		/// <param name="spriteBatch">The destination drawing surface</param>
		/// <param name="x">The X coord of the left side</param>
		/// <param name="y">The Y coord of the upper side</param>
		/// <param name="width">Width</param>
		/// <param name="height">Height</param>
		/// <param name="color">The color to draw the rectangle in</param>
		public static void FillRectangle(this SpriteBatch spriteBatch, float x, float y, float width, float height, Color color)
		{
			FillRectangle(spriteBatch, new Vector2(x, y), new SizeF(width, height), color);
		}

		/// <summary>
		/// Draws a rectangle with the thickness provided
		/// </summary>
		/// <param name="spriteBatch">The destination drawing surface</param>
		/// <param name="rectangle">The rectangle to draw</param>
		/// <param name="color">The color to draw the rectangle in</param>
		/// <param name="thickness">The thickness of the lines</param>
		public static void DrawRectangle(this SpriteBatch spriteBatch, RectangleF rectangle, Color color, float thickness = 1f)
		{
			var texture = GetTexture(spriteBatch);
			var topLeft = new Vector2(rectangle.X, rectangle.Y);
			var topRight = new Vector2(rectangle.Right - thickness, rectangle.Y);
			var bottomLeft = new Vector2(rectangle.X, rectangle.Bottom - thickness);
			var horizontalScale = new Vector2(rectangle.Width, thickness);
			var verticalScale = new Vector2(thickness, rectangle.Height);

			spriteBatch.Draw(texture, topLeft, scale: horizontalScale, color: color);
			spriteBatch.Draw(texture, topLeft, scale: verticalScale, color: color);
			spriteBatch.Draw(texture, topRight, scale: verticalScale, color: color);
			spriteBatch.Draw(texture, bottomLeft, scale: horizontalScale, color: color);
		}

		/// <summary>
		/// Draws a rectangle with the thickness provided
		/// </summary>
		/// <param name="spriteBatch">The destination drawing surface</param>
		/// <param name="location">Where to draw</param>
		/// <param name="size">The size of the rectangle</param>
		/// <param name="color">The color to draw the rectangle in</param>
		/// <param name="thickness">The thickness of the line</param>
		public static void DrawRectangle(this SpriteBatch spriteBatch, Vector2 location, SizeF size, Color color, float thickness = 1f)
		{
			DrawRectangle(spriteBatch, new RectangleF(location.X, location.Y, size.Width, size.Height), color, thickness);
		}

		/// <summary>
		/// Draws a line from point1 to point2 with an offset
		/// </summary>
		/// <param name="spriteBatch">The destination drawing surface</param>
		/// <param name="x1">The X coord of the first point</param>
		/// <param name="y1">The Y coord of the first point</param>
		/// <param name="x2">The X coord of the second point</param>
		/// <param name="y2">The Y coord of the second point</param>
		/// <param name="color">The color to use</param>
		/// <param name="thickness">The thickness of the line</param>
		public static void DrawLine(this SpriteBatch spriteBatch, float x1, float y1, float x2, float y2, Color color, float thickness = 1f)
		{
			DrawLine(spriteBatch, new Vector2(x1, y1), new Vector2(x2, y2), color, thickness);
		}

		/// <summary>
		/// Draws a line from point1 to point2 with an offset
		/// </summary>
		/// <param name="spriteBatch">The destination drawing surface</param>
		/// <param name="point1">The first point</param>
		/// <param name="point2">The second point</param>
		/// <param name="color">The color to use</param>
		/// <param name="thickness">The thickness of the line</param>
		public static void DrawLine(this SpriteBatch spriteBatch, Vector2 point1, Vector2 point2, Color color, float thickness = 1f)
		{
			// calculate the distance between the two vectors
			var distance = Vector2.Distance(point1, point2);

			// calculate the angle between the two vectors
			var angle = (float)Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);

			DrawLine(spriteBatch, point1, distance, angle, color, thickness);
		}

		/// <summary>
		/// Draws a line from point1 to point2 with an offset
		/// </summary>
		/// <param name="spriteBatch">The destination drawing surface</param>
		/// <param name="point">The starting point</param>
		/// <param name="length">The length of the line</param>
		/// <param name="angle">The angle of this line from the starting point</param>
		/// <param name="color">The color to use</param>
		/// <param name="thickness">The thickness of the line</param>
		public static void DrawLine(this SpriteBatch spriteBatch, Vector2 point, float length, float angle, Color color, float thickness = 1f)
		{
			var origin = new Vector2(0f, 0.5f);
			var scale = new Vector2(length, thickness);
			spriteBatch.Draw(GetTexture(spriteBatch), point, null, color, angle, origin, scale, SpriteEffects.None, 0);
		}

		/// <summary>
		/// Draws a point at the specified x, y position. The center of the point will be at the position.
		/// </summary>
		public static void DrawPoint(this SpriteBatch spriteBatch, float x, float y, Color color, float size = 1f)
		{
			DrawPoint(spriteBatch, new Vector2(x, y), color, size);
		}

		/// <summary>
		/// Draws a point at the specified position. The center of the point will be at the position.
		/// </summary>
		public static void DrawPoint(this SpriteBatch spriteBatch, Vector2 position, Color color, float size = 1f)
		{
			var scale = Vector2.One * size;
			var offset = new Vector2(0.5f) - new Vector2(size * 0.5f);
			spriteBatch.Draw(GetTexture(spriteBatch), position + offset, color: color, scale: scale);
		}

		/// <summary>
		/// Draw a circle from a <see cref="CircleF"/> shape
		/// </summary>
		/// <param name="spriteBatch">The destination drawing surface</param>
		/// <param name="circle">The circle shape to draw</param>
		/// <param name="sides">The number of sides to generate</param>
		/// <param name="color">The color of the circle</param>
		/// <param name="thickness">The thickness of the lines used</param>
		public static void DrawCircle(this SpriteBatch spriteBatch, CircleF circle, int sides, Color color, float thickness = 1f)
		{
			DrawCircle(spriteBatch, circle.Center, circle.Radius, sides, color, thickness);
		}

		/// <summary>
		/// Draw a circle
		/// </summary>
		/// <param name="spriteBatch">The destination drawing surface</param>
		/// <param name="center">The center of the circle</param>
		/// <param name="radius">The radius of the circle</param>
		/// <param name="sides">The number of sides to generate</param>
		/// <param name="color">The color of the circle</param>
		/// <param name="thickness">The thickness of the lines used</param>
		public static void DrawCircle(this SpriteBatch spriteBatch, Vector2 center, float radius, int sides, Color color, float thickness = 1f)
		{
			DrawPolygon(spriteBatch, center, CreateCircle(radius, sides), color, thickness);
		}

		/// <summary>
		/// Draw a circle
		/// </summary>
		/// <param name="spriteBatch">The destination drawing surface</param>
		/// <param name="x">The center X of the circle</param>
		/// <param name="y">The center Y of the circle</param>
		/// <param name="radius">The radius of the circle</param>
		/// <param name="sides">The number of sides to generate</param>
		/// <param name="color">The color of the circle</param>
		/// <param name="thickness">The thickness of the line</param>
		public static void DrawCircle(this SpriteBatch spriteBatch, float x, float y, float radius, int sides, Color color, float thickness = 1f)
		{
			DrawPolygon(spriteBatch, new Vector2(x, y), CreateCircle(radius, sides), color, thickness);
		}

		private static Vector2[] CreateCircle(double radius, int sides)
		{
			const double max = 2.0 * Math.PI;
			var points = new Vector2[sides];
			var step = max / sides;
			var theta = 0.0;

			for (var i = 0; i < sides; i++)
			{
				points[i] = new Vector2((float)(radius * Math.Cos(theta)), (float)(radius * Math.Sin(theta)));
				theta += step;
			}

			return points;
		}
	}
}