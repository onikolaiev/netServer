using System;
using System.Linq;
using WCell.Util;
using WCell.Util.Graphics;

namespace WCell.Terrain.Legacy
{
	public static class Intersection
	{
		// Fields
		public const float Epsilon = 1E-05f;

		// Methods
		public static void Barycentric(Vector3 a, Vector3 b, Vector3 c, Vector3 p, out Vector3 uvw)
		{
			var vector = b - a;
			var vector2 = c - a;
			var vector3 = p - a;
			var num = Vector3.Dot(vector, vector);
			var num2 = Vector3.Dot(vector, vector2);
			var num3 = Vector3.Dot(vector2, vector2);
			var num4 = Vector3.Dot(vector3, vector);
			var num5 = Vector3.Dot(vector3, vector2);
			var num6 = (num * num3) - (num2 * num2);
			uvw = new Vector3();
			if (num6 != 0f)
			{
				uvw.Y = ((num3 * num4) - (num2 * num5)) / num6;
				uvw.Z = ((num * num5) - (num2 * num4)) / num6;
				uvw.X = (1f - uvw.Y) - uvw.Z;
			}
		}

		public static void Barycentric2(Vector3 a, Vector3 b, Vector3 c, Vector3 p, out Vector2 uv)
		{
			float num;
			float num2;
			float num3;
			uv = new Vector2();
			var vector = Vector3.Cross(b - a, c - a);
			var num4 = Math.Abs(vector.X);
			var num5 = Math.Abs(vector.Y);
			var num6 = Math.Abs(vector.Z);
			if ((num4 >= num5) && (num4 >= num6))
			{
				num = TriArea2D(p.Y, p.Z, b.Y, b.Z, c.Y, c.Z);
				num2 = TriArea2D(p.Y, p.Z, c.Y, c.Z, a.Y, a.Z);
				num3 = 1f / vector.X;
			}
			else if ((num5 >= num4) && (num5 >= num6))
			{
				num = TriArea2D(p.X, p.Z, b.X, b.Z, c.X, c.Z);
				num2 = TriArea2D(p.X, p.Z, c.X, c.Z, a.X, a.Z);
				num3 = 1f / -vector.Y;
			}
			else
			{
				num = TriArea2D(p.X, p.Y, b.X, b.Y, c.X, c.Y);
				num2 = TriArea2D(p.X, p.Y, c.X, c.Y, a.X, a.Y);
				num3 = 1f / vector.Z;
			}
			uv.X = num * num3;
			uv.Y = num2 * num3;
		}

		public static float Clamp(float n, float min, float max)
		{
			if (n < min)
			{
				return min;
			}
			if (n > max)
			{
				return max;
			}
			return n;
		}

		public static Vector3 ClosestPointOnPlane(Vector3 point, Plane plane)
		{
			return (point - plane.Normal * plane.DotCoordinate(point));
		}

		public static void ClosestPointOnSegment(Vector3 c, Vector3 a, Vector3 b, out float t, out Vector3 d)
		{
			var vector = b - a;
			t = Vector3.Dot(c - a, vector) / Vector3.Dot(vector, vector);
			if (t < 0f)
			{
				t = 0f;
			}
			if (t > 1f)
			{
				t = 1f;
			}
			d = a + (t * vector);
		}

		/// <summary>
		/// See http://paulbourke.net/geometry/pointline/
		/// Finds p = the closest point to c on the line from a to b.
		/// Also finds t = the distance from a to that point on the line.
		/// </summary>
		public static void ClosestPointOnLine(Vector3 c, Vector3 a, Vector3 b, out float t, out Vector3 p)
		{
			var direction = b - a;
			t = Vector3.Dot(c - a, direction) / Vector3.Dot(direction, direction);
			p = a + (t * direction);
		}

		/// <summary>
		/// Finds p = the closest point to c on the line from a to b.
		/// Also finds t = the distance from a to that point on the line.
		/// </summary>
		public static void ClosestPointOnLine(Vector2 c, Vector2 a, Vector2 b, out float t, out Vector2 p)
		{
			var direction = b - a;
			t = Vector2.Dot(c - a, direction) / Vector2.Dot(direction, direction);
			p = a + (t * direction);
		}

		/// <summary>
		/// Finds the direction from a to the closest point to c on the line from a to b.
		/// </summary>
		public static float DirectionToPoint(Vector2 c, Vector2 a, Vector2 b)
		{
			var direction = b - a;
			return Vector2.Dot(c - a, direction) / Vector2.Dot(direction, direction);
		}

		public static void ClosestPtPointAABB(Vector4 p, BoundingBox b, ref Vector3 q)
		{
			var x = p.X;
			if (x < b.Min.X)
			{
				x = b.Min.X;
			}
			if (x > b.Max.X)
			{
				x = b.Max.X;
			}
			q.X = x;
			if (x < b.Min.Y)
			{
				x = b.Min.Y;
			}
			if (x > b.Max.Y)
			{
				x = b.Max.Y;
			}
			q.Y = x;
			if (x < b.Min.Z)
			{
				x = b.Min.Z;
			}
			if (x > b.Max.Z)
			{
				x = b.Max.Z;
			}
			q.Z = x;
		}

		public static void ClosestPtPointRect(Vector3 p, Vector3 a, Vector3 b, Vector3 c, ref Vector3 q)
		{
			var vector = b - a;
			var vector2 = c - a;
			var vector3 = p - a;
			q = a;
			var num = Vector3.Dot(vector3, vector);
			var num2 = Vector3.Dot(vector, vector);
			if (num >= num2)
			{
				q += vector;
			}
			else if (num > 0f)
			{
				q = q + ((num / num2) * vector);
			}
			num = Vector3.Dot(vector3, vector2);
			num2 = Vector3.Dot(vector2, vector2);
			if (num >= num2)
			{
				q += vector2;
			}
			else if (num > 0f)
			{
				q = q + ((num / num2) * vector2);
			}
		}

		public static void ClosestPtPointSegment2D(Vector2 c, Vector2 a, Vector2 b, ref float t, ref Vector2 d)
		{
			var vector = b - a;
			t = Vector2.Dot(c - a, vector);
			if (t <= 0f)
			{
				t = 0f;
				d = a;
			}
			else
			{
				var num = Vector2.Dot(vector, vector);
				if (t >= num)
				{
					t = 1f;
					d = b;
				}
				else
				{
					t /= num;
					d = a + (t * vector);
				}
			}
		}

		public static Vector3 ClosestPtPointTriangle(Vector3 p, Vector3 a, Vector3 b, Vector3 c)
		{
			var vector = b - a;
			var vector2 = c - a;
			var vector3 = c - b;
			var num = Vector3.Dot(p - a, vector);
			var num2 = Vector3.Dot(p - b, a - b);
			var num3 = Vector3.Dot(p - a, vector2);
			var num4 = Vector3.Dot(p - c, a - c);
			if ((num <= 0f) && (num3 <= 0f))
			{
				return a;
			}
			var num5 = Vector3.Dot(p - b, vector3);
			var num6 = Vector3.Dot(p - c, b - c);
			if ((num2 <= 0f) && (num5 <= 0f))
			{
				return b;
			}
			if ((num4 <= 0f) && (num6 <= 0f))
			{
				return c;
			}
			var vector4 = Vector3.Cross(b - a, c - a);
			var num7 = Vector3.Dot(vector4, Vector3.Cross(a - p, b - p));
			if (((num7 <= 0f) && (num >= 0f)) && (num2 >= 0f))
			{
				return (a + (((num / (num + num2)) * vector)));
			}
			var num8 = Vector3.Dot(vector4, Vector3.Cross(b - p, c - p));
			if (((num8 <= 0f) && (num5 >= 0f)) && (num6 >= 0f))
			{
				return (b + (((num5 / (num5 + num6)) * vector3)));
			}
			var num9 = Vector3.Dot(vector4, Vector3.Cross(c - p, a - p));
			if (((num9 <= 0f) && (num3 >= 0f)) && (num4 >= 0f))
			{
				return (a + (((num3 / (num3 + num4)) * vector2)));
			}
			var num10 = num8 / ((num8 + num9) + num7);
			var num11 = num9 / ((num8 + num9) + num7);
			var num12 = (1f - num10) - num11;
			return (((num10 * a) + (num11 * b)) + (num12 * c));
		}

		public static Vector3 ClosestPtPointTriangleNew(Vector3 p, Vector3 a, Vector3 b, Vector3 c)
		{
			float num10;
			var vector = b - a;
			var vector2 = c - a;
			var vector3 = p - a;
			var num = Vector3.Dot(vector, vector3);
			var num2 = Vector3.Dot(vector2, vector3);
			if ((num <= 0f) && (num2 <= 0f))
			{
				return a;
			}
			var vector4 = p - b;
			var num3 = Vector3.Dot(vector, vector4);
			var num4 = Vector3.Dot(vector2, vector4);
			if ((num3 >= 0f) && (num4 <= num3))
			{
				return b;
			}
			var num5 = (num * num4) - (num3 * num2);
			if (((num5 <= 0f) && (num >= 0f)) && (num3 <= 0f))
			{
				var num6 = num / (num - num3);
				return (a + ((num6 * vector)));
			}
			var vector5 = p - c;
			var num7 = Vector3.Dot(vector, vector5);
			var num8 = Vector3.Dot(vector2, vector5);
			if ((num8 >= 0f) && (num7 <= num8))
			{
				return c;
			}
			var num9 = (num7 * num2) - (num * num8);
			if (((num9 <= 0f) && (num2 >= 0f)) && (num8 <= 0f))
			{
				num10 = num2 / (num2 - num8);
				return (a + ((num10 * vector2)));
			}
			var num11 = (num3 * num8) - (num7 * num4);
			if (((num11 <= 0f) && ((num4 - num3) >= 0f)) && ((num7 - num8) >= 0f))
			{
				num10 = (num4 - num3) / ((num4 - num3) + (num7 - num8));
				return (b + ((num10 * (c - b))));
			}
			var num12 = 1f / ((num11 + num9) + num5);
			var num13 = num9 * num12;
			var num14 = num5 * num12;
			return ((a + (vector * num13)) + (vector2 * num14));
		}

		public static Vector3 Corner(BoundingBox b, int n)
		{
			var vector = new Vector3
			{
				X = ((n & 1) == 1) ? b.Max.X : b.Min.X,
				Y = ((n & 2) == 2) ? b.Max.Y : b.Min.Y,
				Z = ((n & 4) == 4) ? b.Max.Z : b.Min.Z
			};
			return vector;
		}

		public static float DistPointPlane(Vector3 q, Plane p)
		{
			return ((Vector3.Dot(p.Normal, q) - p.D) / Vector3.Dot(p.Normal, p.Normal));
		}

		public static bool IntersectionOfTwoLines(Vector3 a, Vector3 b, Vector3 c, Vector3 d, ref Vector3 result)
		{
			double num3 = ((b.X - a.X) * (d.Y - c.Y)) - ((b.Y - a.Y) * (d.X - c.X));
			if (num3 == 0.0)
			{
				return false;
			}
			double num4 = ((a.Y - c.Y) * (d.X - c.X)) - ((a.X - c.X) * (d.Y - c.Y));
			var num = num4 / num3;
			double num5 = ((a.Y - c.Y) * (b.X - a.X)) - ((a.X - c.X) * (b.Y - a.Y));
			var num2 = num5 / num3;
			if ((((num < 0.0) || (num > 1.0)) || (num2 < 0.0)) || (num2 > 1.0))
			{
				return false;
			}
			result.X = a.X + ((float)(num * (b.X - a.X)));
			result.Y = a.Y + ((float)(num * (b.Y - a.Y)));
			return true;
		}

		public static bool IntersectMovingSpherePlane(BoundingSphere s, Vector3 v, Plane p, ref float t, ref Vector3 q)
		{
			var num = Vector3.Dot(p.Normal, s.Center) - p.D;
			if (Math.Abs(num) <= s.Radius)
			{
				t = 0f;
				q = s.Center;
				return true;
			}
			var num2 = Vector3.Dot(p.Normal, v);
			if ((num2 * num) >= 0f)
			{
				return false;
			}
			var num3 = (num > 0f) ? s.Radius : -s.Radius;
			t = (num3 - num) / num2;
			if (t > 1f)
			{
				return false;
			}
			q = (s.Center + (t * v)) - ((num3 * p.Normal));
			return true;
		}

		public static bool IntersectPlanes(Plane p1, Plane p2, Plane p3, ref Vector3 p)
		{
			var vector = Vector3.Cross(p2.Normal, p3.Normal);
			var num = Vector3.Dot(p1.Normal, vector);
			if (Math.Abs(num) < 0.0001f)
			{
				return false;
			}
			p = (((p1.D * vector) + Vector3.Cross(p1.Normal, ((p3.D * p2.Normal) - (p2.D * p3.Normal)))) / num);
			return true;
		}

		public static bool IntersectPlanes(Plane p1, Plane p2, ref Vector3 p, ref Vector3 d)
		{
			d = Vector3.Cross(p1.Normal, p2.Normal);
			var num = Vector3.Dot(d, d);
			if (num < 1E-05f)
			{
				return false;
			}
			p = (Vector3.Cross(((p1.D * p2.Normal) - (p2.D * p1.Normal)), d) / num);
			return true;
		}

		public static bool IntersectRaySphere(Vector3 p, Vector3 d, BoundingSphere s, ref float t, ref Vector3 q)
		{
			var vector = p - s.Center;
			var num = Vector3.Dot(vector, d);
			var num2 = Vector3.Dot(vector, vector) - (s.Radius * s.Radius);
			if ((num2 > 0f) && (num > 0f))
			{
				return false;
			}
			var num3 = (num * num) - num2;
			if (num3 < 0f)
			{
				return false;
			}
			t = -num - ((float)Math.Sqrt(num3));
			if (t < 0f)
			{
				t = 0f;
			}
			q = p + (t * d);
			return true;
		}

		public static bool IntersectSegmentCylinder(Vector3 sa, Vector3 sb, Vector3 p, Vector3 q, float r, ref float t)
		{
			var vector = q - p;
			var vector2 = sa - p;
			var vector3 = sb - sa;
			var num = Vector3.Dot(vector2, vector);
			var num2 = Vector3.Dot(vector3, vector);
			var num3 = Vector3.Dot(vector, vector);
			if ((num >= 0f) || ((num + num2) >= 0f))
			{
				if ((num > num3) && ((num + num2) > num3))
				{
					return false;
				}
				var num4 = Vector3.Dot(vector3, vector3);
				var num5 = Vector3.Dot(vector2, vector3);
				var num6 = (num3 * num4) - (num2 * num2);
				var num7 = Vector3.Dot(vector2, vector2) - (r * r);
				var num8 = (num3 * num7) - (num * num);
				if (num6 < 0.0001f)
				{
					if (num8 > 0f)
					{
						return false;
					}
					if (num < 0f)
					{
						t = -num5 / num4;
					}
					else if (num > num3)
					{
						t = (num2 - num5) / num4;
					}
					else
					{
						t = 0f;
					}
					return true;
				}
				var num9 = (num3 * num5) - (num2 * num);
				var num10 = (num9 * num9) - (num6 * num8);
				if (num10 >= 0f)
				{
					t = (-num9 - ((float)Math.Sqrt(num10))) / num6;
					if ((t < 0f) || (t > 1f))
					{
						return false;
					}
					if ((num + (t * num2)) < 0f)
					{
						if (num2 <= 0f)
						{
							return false;
						}
						t = -num / num2;
						return ((num7 + ((2f * t) * (num5 + (t * num4)))) <= 0f);
					}
					if ((num + (t * num2)) > num3)
					{
						if (num2 >= 0f)
						{
							return false;
						}
						t = (num3 - num) / num2;
						return ((((num7 + num3) - (2f * num)) + (t * ((2f * (num5 - num2)) + (t * num4)))) <= 0f);
					}
				}
			}
			return false;
		}

		public static bool IntersectSegmentPlane(Vector3 a, Vector3 b, Plane p, ref float t, ref Vector3 q)
		{
			var vector = b - a;
			t = (p.D - Vector3.Dot(p.Normal, a)) / Vector3.Dot(p.Normal, vector);
			if ((t >= 0f) && (t <= 1f))
			{
				q = a + (t * vector);
				return true;
			}
			return false;
		}

		public static bool IntersectSegmentPolyhedron(Vector3 a, Vector3 b, Plane[] p, int n, ref float tfirst, ref float tlast)
		{
			var vector = b - a;
			tfirst = 0f;
			tlast = 1f;
			for (var i = 0; i < n; i++)
			{
				var num2 = Vector3.Dot(p[i].Normal, vector);
				var num3 = p[i].D - Vector3.Dot(p[i].Normal, a);
				if (num2 == 0f)
				{
					if (num3 > 0f)
					{
						return false;
					}
				}
				else
				{
					var num4 = num3 / num2;
					if (num2 < 0f)
					{
						if (num4 > tfirst)
						{
							tfirst = num4;
						}
					}
					else if (num4 < tlast)
					{
						tlast = num4;
					}
					if (tfirst > tlast)
					{
						return false;
					}
				}
			}
			return true;
		}

		public static bool IntersectSegmentTriangle(Vector3 p, Vector3 q, Vector3 a, Vector3 b, Vector3 c, ref float u, ref float v, ref float w, ref float t)
		{
			var vector = b - a;
			var vector2 = c - a;
			var vector3 = p - q;
			var vector4 = Vector3.Cross(vector, vector2);
			var num = Vector3.Dot(vector3, vector4);
			if (num <= 0f)
			{
				return false;
			}
			var vector5 = p - a;
			t = Vector3.Dot(vector5, vector4);
			if (t < 0f)
			{
				return false;
			}
			if (t > num)
			{
				return false;
			}
			var vector6 = Vector3.Cross(vector3, vector5);
			v = Vector3.Dot(vector2, vector6);
			if ((v < 0f) || (v > num))
			{
				return false;
			}
			w = -Vector3.Dot(vector, vector6);
			if ((w < 0f) || ((v + w) > num))
			{
				return false;
			}
			var num2 = 1f / num;
			t *= num2;
			v *= num2;
			w *= num2;
			u = (1f - v) - w;
			return true;
		}

		public static bool IntersectSegmentRectangle2DXY(Rect rect, Vector3 point1, Vector3 point2)
		{
			var rectMin = new Vector2(rect.TopLeft.X, rect.TopLeft.Y);
			var rectMax = new Vector2(rect.BottomRight.X, rect.BottomRight.Y);
			var point12d = new Vector2(point1.X, point2.X);
			var point22d = new Vector2(point2.X, point2.Y);

			return IntersectSegmentRectangle2D(rectMin, rectMax, point12d, point22d);
		}

		public static bool IntersectSegmentRectangle2D(Vector2 rectMin, Vector2 rectMax, Vector2 point1, Vector2 point2)
		{
			// Find min and max X for the segment
			var a_rectangleMinX = rectMin.X;
			var a_rectangleMinY = rectMin.Y;
			var a_rectangleMaxX = rectMax.X;
			var a_rectangleMaxY = rectMax.Y;
			var a_p1x = point1.X;
			var a_p1y = point1.Y;
			var a_p2x = point2.X;
			var a_p2y = point2.Y;

			var minX = a_p1x;
			var maxX = a_p2x;
			if (a_p1x > a_p2x)
			{
				minX = a_p2x;
				maxX = a_p1x;
			}

			// Find the intersection of the segment's and rectangle's x-projections
			if (maxX > a_rectangleMaxX)
			{
				maxX = a_rectangleMaxX;
			}

			if (minX < a_rectangleMinX)
			{
				minX = a_rectangleMinX;
			}

			if (minX > maxX) // If their projections do not intersect return false
			{
				return false;
			}

			// Find corresponding min and max Y for min and max X we found before

			var minY = a_p1y;
			var maxY = a_p2y;

			var dx = a_p2x - a_p1x;

			if (Math.Abs(dx) > Epsilon)
			{
				var a = (a_p2y - a_p1y) / dx;
				var b = a_p1y - a * a_p1x;
				minY = a * minX + b;
				maxY = a * maxX + b;
			}

			if (minY > maxY)
			{
				var tmp = maxY;
				maxY = minY;
				minY = tmp;
			}

			// Find the intersection of the segment's and rectangle's y-projections
			if (maxY > a_rectangleMaxY)
			{
				maxY = a_rectangleMaxY;
			}

			if (minY < a_rectangleMinY)
			{
				minY = a_rectangleMinY;
			}

			return (minY <= maxY);
		}

		public static bool IsConvexQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
		{
			var vector = Vector3.Cross(d - b, a - b);
			var vector2 = Vector3.Cross(d - b, c - b);
			if (Vector3.Dot(vector, vector2) >= 0f)
			{
				return false;
			}
			var vector3 = Vector3.Cross(c - a, d - a);
			var vector4 = Vector3.Cross(c - a, b - a);
			return (Vector3.Dot(vector3, vector4) < 0f);
		}

		public static bool PointInsideEdge(ref Vector3 edgeA, ref Vector3 edgeB, ref Vector3 point)
		{
			return (Vector3.Dot(edgeA - point, edgeB - point) <= Epsilon);
		}

		public static bool PointInsidePoly(ref Vector3 vA, ref Vector3 vB, ref Vector3 vC, ref Vector3 p)
		{
			var num = 0;
			var num2 = 0;
			var vectorArray = new[] { vA, vB, vC };
			var plane = new Plane(vA, vB, vC);
			uint index = 2;
			for (uint i = 0; i < 3; i++)
			{
				var vector = vectorArray[index];
				var vector2 = vectorArray[i];
				var vector3 = Vector3.Cross(vector2 - vector, plane.Normal);
				var num5 = Vector3.Dot(p, vector3) - Vector3.Dot(vector, vector3);
				if (num5 > Epsilon)
				{
					num++;
				}
				else if (num5 < -Epsilon)
				{
					num2++;
				}
				if ((num | num2) != 0)
				{
					return false;
				}
				index = i;
			}
			return ((num | num2) == 0);
		}

		public static bool PointInTriangle(Vector3 p, Vector3 a, Vector3 b, Vector3 c)
		{
			a -= p;
			b -= p;
			c -= p;
			var vector = Vector3.Cross(b, c);
			var vector2 = Vector3.Cross(c, a);
			if (Vector3.Dot(vector, vector2) < 0f)
			{
				return false;
			}
			var vector3 = Vector3.Cross(a, b);
			if (Vector3.Dot(vector, vector3) < 0f)
			{
				return false;
			}
			return true;
		}

		public static bool PointInTriangle(ref Vector2 p, ref Vector2 a, ref Vector2 b, ref Vector2 c)
		{
			if (Vector2.Dot(p - a, b - a) < 0f)
			{
				return false;
			}
			if (Vector2.Dot(p - b, c - b) < 0f)
			{
				return false;
			}
			if (Vector2.Dot(p - c, a - c) < 0f)
			{
				return false;
			}
			return true;
		}

		public static bool PointInTriangle2D(ref Vector2 p, ref Vector2 a, ref Vector2 b, ref Vector2 c)
		{
			var num = Vector2.Dot(p - a, b - a);
			var num2 = Vector2.Dot(p - b, c - b);
			if (Math.Sign(num) != Math.Sign(num2))
			{
				return false;
			}
			var num3 = Vector2.Dot(p - c, a - c);
			if (Math.Sign(num) != Math.Sign(num3))
			{
				return false;
			}
			return true;
		}

		/// <summary>
		/// Checks if point P is inside Triangle ABC using only the X and Y coordinates of the Vector3.
		/// </summary>
		/// <param name="p"></param>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <param name="c"></param>
		/// <returns></returns>
		public static bool PointInTriangle2DXY(Vector3 p, Vector3 a, Vector3 b, Vector3 c)
		{
			var a2d = new Vector2(a.X, a.Y);
			var b2d = new Vector2(b.X, b.Y);
			var c2d = new Vector2(c.X, c.Y);
			var p2d = new Vector2(p.X, p.Y);

			return PointInTriangle2D(ref p2d, ref a2d, ref b2d, ref c2d);
		}

		public static bool PointInTriangle2DXY(Point p, Vector3 a, Vector3 b, Vector3 c)
		{
			var a2d = new Vector2(a.X, a.Y);
			var b2d = new Vector2(b.X, b.Y);
			var c2d = new Vector2(c.X, c.Y);
			var p2d = new Vector2(p.X, p.Y);

			return PointInTriangle2D(ref p2d, ref a2d, ref b2d, ref c2d);
		}

		public static bool PointInTriangleBarycentric(Vector3 a, Vector3 b, Vector3 c, Vector3 p)
		{
			Vector3 vector;
			Barycentric(a, b, c, p, out vector);
			return (((vector.Y >= 0f) && (vector.Z >= 0f)) && ((vector.Y + vector.Z) <= 1f));
		}

		/// <summary>
		/// Checks whether the given ray *hits the front face* of the given triangle 
		/// </summary>
		public static bool RayTriangleIntersect(Ray r, Vector3 t1, Vector3 t2, Vector3 t3, out float distance)
		{
			distance = 0f;
			var s1 = t2 - t1;
			var s2 = t3 - t1;
			var vector4 = Vector3.Cross(r.Direction, s2);
			var num = Vector3.Dot(s1, vector4);
			if (num > -1E-05f)		// check that ray is above s2 segment
			{
				return false;
			}
			var num2 = 1f / num;
			var vector3 = r.Position - t1;
			var num3 = Vector3.Dot(vector3, vector4) * num2;
			if ((num3 < -0.001f) || (num3 > 1.001f))
			{
				return false;
			}
			var vector5 = Vector3.Cross(vector3, s1);
			var num4 = Vector3.Dot(r.Direction, vector5) * num2;
			if ((num4 < -0.001f) || ((num3 + num4) > 1.001f))
			{
				return false;
			}
			distance = Vector3.Dot(s2, vector5) * num2;
			if (distance <= 0f)
			{
				return false;
			}
			return true;
		}

		/// <summary>
		/// Intersects a ray with the given triangle
		/// I think it does not currently work?
		/// </summary>
		/// <param name="dist">The distance of the given point</param>
		public static bool RayTriangleIntersect2(Ray ray, Triangle tri, out float dist)
		{
			// See http://www.cs.princeton.edu/courses/archive/fall00/cs426/lectures/raycast/sld017.htm
			var normal = tri.CalcNormalizedNormal();
			var d = Vector3.Dot(normal, tri.Point1);
			dist = -(Vector3.Dot(ray.Position, normal) + d) / (Vector3.Dot(ray.Direction, normal));

			if (float.IsNaN(dist) || dist < 0)
			{
				// no intersection
				return false;
			}

			// intersection point
			var p = ray.Position + ray.Direction * dist;

			// See http://www.cs.princeton.edu/courses/archive/fall00/cs426/lectures/raycast/sld018.htm

			var n1 = Vector3.Cross(tri.Point1 - ray.Position, tri.Point2 - ray.Position);
			n1.Normalize();
			var d1 = -Vector3.Dot(ray.Position, n1);
			if (Vector3.Dot(p, n1) + d1 < 0)
			{
				return false;
			}

			var n2 = Vector3.Cross(tri.Point2 - ray.Position, tri.Point3 - ray.Position);
			n2.Normalize();
			var d2 = -Vector3.Dot(ray.Position, n2);
			if (Vector3.Dot(p, n2) + d2 < 0)
			{
				return false;
			}

			var n3 = Vector3.Cross(tri.Point3 - ray.Position, tri.Point1 - ray.Position);
			n3.Normalize();
			var d3 = -Vector3.Dot(ray.Position, n3);
			if (Vector3.Dot(p, n3) + d3 < 0)
			{
				return false;
			}
			


			return true;
		}

		public static bool RayTriangleIntersect(Vector3 ray_origin, Vector3 ray_direction, Vector3 vert0, Vector3 vert1, Vector3 vert2, out float t, out float u, out float v)
		{
			t = 0f;
			u = 0f;
			v = 0f;
			var edge1 = vert1 - vert0;
			var edge2 = vert2 - vert0;
			var vector4 = Vector3.Cross(ray_direction, edge2);
			var num = Vector3.Dot(edge1, vector4);
			if (num > -1E-05f)
			{
				return false;
			}
			var num2 = 1f / num;
			var vector3 = ray_origin - vert0;
			u = Vector3.Dot(vector3, vector4) * num2;
			if ((u < -0.00001f) || (u > 1.00001f))
			{
				return false;
			}
			var vector5 = Vector3.Cross(vector3, edge1);
			v = Vector3.Dot(ray_direction, vector5) * num2;
			if ((v < -0.00001f) || ((u + v) > 1.00001f))
			{
				return false;
			}
			t = Vector3.Dot(edge2, vector5) * num2;
			return t > 0.0f;
		}

		public static float Signed2DTriArea(Vector2 a, Vector2 b, Vector2 c)
		{
			return (((a.X - c.X) * (b.Y - c.Y)) - ((a.Y - c.Y) * (b.X - c.X)));
		}

		public static float SqDistPointAABB(Vector3 p, BoundingBox b)
		{
			var num = 0f;
			var x = p.X;
			if (x < b.Min.X)
			{
				num += (b.Min.X - x) * (b.Min.X - x);
			}
			if (x > b.Max.X)
			{
				num += (x - b.Max.X) * (x - b.Max.X);
			}
			x = p.Y;
			if (x < b.Min.Y)
			{
				num += (b.Min.Y - x) * (b.Min.Y - x);
			}
			if (x > b.Max.Y)
			{
				num += (x - b.Max.Y) * (x - b.Max.Y);
			}
			x = p.Z;
			if (x < b.Min.Z)
			{
				num += (b.Min.Z - x) * (b.Min.Z - x);
			}
			if (x > b.Max.Z)
			{
				num += (x - b.Max.Z) * (x - b.Max.Z);
			}
			return num;
		}

		public static float SqDistPointSegment(Vector3 a, Vector3 b, Vector3 c)
		{
			var vector = b - a;
			var vector2 = c - a;
			var vector3 = c - b;
			var num = Vector3.Dot(vector2, vector);
			if (num <= 0f)
			{
				return Vector3.Dot(vector2, vector2);
			}
			var num2 = Vector3.Dot(vector, vector);
			if (num >= num2)
			{
				return Vector3.Dot(vector3, vector3);
			}
			return (Vector3.Dot(vector2, vector2) - ((num * num) / num2));
		}

		public static bool Test2DSegmentSegment(Vector2 a, Vector2 b, Vector2 c, Vector2 d, ref float t, ref Vector2 p)
		{
			var num = Signed2DTriArea(a, b, d);
			var num2 = Signed2DTriArea(a, b, c);
			if ((num * num2) < 0f)
			{
				var num3 = Signed2DTriArea(c, d, a);
				var num4 = (num3 + num2) - num;
				if ((num3 * num4) < 0f)
				{
					t = num3 / (num3 - num4);
					p = a + (t * (b - a));
					return true;
				}
			}
			return false;
		}

		public static bool TestMovingSpherePlane(Vector3 a, Vector3 b, float r, Plane p)
		{
			var num = Vector3.Dot(a, p.Normal) - p.D;
			var num2 = Vector3.Dot(b, p.Normal) - p.D;
			return (((num * num2) < 0f) || ((Math.Abs(num) <= r) || (Math.Abs(num2) <= r)));
		}

		public static bool TestMovingSphereSphere(BoundingSphere s0, BoundingSphere s1, Vector3 v0, Vector3 v1, ref float t)
		{
			var vector = s1.Center - s0.Center;
			var vector2 = v1 - v0;
			var num = s1.Radius + s0.Radius;
			var num2 = Vector3.Dot(vector, vector) - (num * num);
			if (num2 < 0f)
			{
				t = 0f;
				return true;
			}
			var num3 = Vector3.Dot(vector2, vector2);
			if (num3 < 0.0001f)
			{
				return false;
			}
			var num4 = Vector3.Dot(vector2, vector);
			if (num4 >= 0f)
			{
				return false;
			}
			var num5 = (num4 * num4) - (num3 * num2);
			if (num5 < 0f)
			{
				return false;
			}
			t = (-num4 - ((float)Math.Sqrt(num5))) / num3;
			return true;
		}

		public static bool TestPointPolyhedron(Vector3 p, Plane[] planes)
		{
			return planes.All(t => (DistPointPlane(p, t) <= 0f));
		}

		public static bool TestRaySphere(Vector3 p, Vector3 d, BoundingSphere s)
		{
			var vector = p - s.Center;
			var num = Vector3.Dot(vector, vector) - (s.Radius * s.Radius);
			if (num > 0f)
			{
				var num2 = Vector3.Dot(vector, d);
				if (num2 > 0f)
				{
					return false;
				}
				var num3 = (num2 * num2) - num;
				if (num3 < 0f)
				{
					return false;
				}
			}
			return true;
		}

		public static float TriArea2D(Vector2 p1, Vector2 p2, Vector2 p3)
		{
			return (((p1.X - p2.X) * (p2.Y - p3.Y)) - ((p2.X - p3.X) * (p1.Y - p2.Y)));
		}

		public static float TriArea2D(float x1, float y1, float x2, float y2, float x3, float y3)
		{
			return (((x1 - x2) * (y2 - y3)) - ((x2 - x3) * (y1 - y2)));
		}

		public static int Vector3FarthestFromEdge(Vector2 a, Vector2 b, Vector2[] p, int n)
		{
			var vector = b - a;
			var vector2 = new Vector2(-vector.Y, vector.X);
			var num = -1;
			var minValue = Single.MinValue;
			var num3 = Single.MinValue;
			for (var i = 1; i < n; i++)
			{
				var num5 = Vector2.Dot(p[i] - a, vector2);
				var num6 = Vector2.Dot(p[i] - a, vector);
				if ((num5 <= minValue) && ((num5 != minValue) || (num6 <= num3))) continue;

				num = i;
				minValue = num5;
				num3 = num6;
			}
			return num;
		}

		/// <summary>
		/// Determines the intersection of a line segment between v1 and v2 and the given plane.
		/// Returns false if the plane is outside the line segment or parallel to it.
		/// </summary>
		public static bool LineSegmentIntersectsPlane(Vector3 v1, Vector3 v2, Plane plane, out Vector3 pos)
		{
			var vector = v2 - v1;
			var direction = Vector3.Normalize(vector);
			var ray = new Ray(v1, direction);
			var dist = ray.Intersect(plane);
			if (!float.IsNaN(dist) && dist * dist < vector.LengthSquared())
			{
				pos = ray.Position + (ray.Direction * dist);
				return true;
			}
			pos = Vector3.Zero;
			return false;
		}

		/// <summary>
		/// Determines the closest point on the ray from v1 to v2 and the given plane.
		/// Returns false if the plane is parallel to the line.
		/// </summary>
		public static bool ClosestPointToPlane(Vector3 v1, Vector3 v2, Plane plane, out Vector3 pos)
		{
			var direction = v2 - v1;
			var ray = new Ray(v1, direction);
			var dist = ray.Intersect(plane);
			if (!float.IsNaN(dist))
			{
				// clamp between 0 and 1
				dist = MathUtil.ClampMinMax(dist, 0, 1);
				pos = ray.Position + (direction * dist);
				return true;
			}
			pos = Vector3.Zero;
			return false;
		}

		/// <summary>
		/// Calculates the point on the segment going through the given two line points that is closest to the given point.
		/// Based on: http://www.gamedev.net/topic/444154-closest-point-on-a-line/
		/// </summary>
		public static Vector3 FindClosestPointInSegment(Vector3 segmentA, Vector3 segmentB, Vector3 point)
		{
			var AP = point - segmentA;
			var AB = segmentB - segmentA;

			var abSq = AB.LengthSquared();

			var ap_ab = AP.X * AB.X + AP.Y * AB.Y;
			var t = ap_ab / abSq;

			// clamp
			if (t < 0.0f) t = 0.0f;
			else if (t > 1.0f) t = 1.0f;

			return segmentA + (AB * t);
		}
	}
}
