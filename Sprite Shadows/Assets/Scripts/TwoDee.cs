using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace TwoDee {
    public class Intersection {
        public static bool LineSegmentsIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector3 p4, out Vector2 intersection) {
            intersection = Vector2.zero;

            var d = (p2.x - p1.x) * (p4.y - p3.y) - (p2.y - p1.y) * (p4.x - p3.x);

            if (d == 0.0f) {
                return false;
            }

            var u = ((p3.x - p1.x) * (p4.y - p3.y) - (p3.y - p1.y) * (p4.x - p3.x)) / d;
            var v = ((p3.x - p1.x) * (p2.y - p1.y) - (p3.y - p1.y) * (p2.x - p1.x)) / d;

            if (u < 0.0f || u > 1.0f || v < 0.0f || v > 1.0f) {
                return false;
            }

            intersection.x = p1.x + u * (p2.x - p1.x);
            intersection.y = p1.y + u * (p2.y - p1.y);

            return true;
        }

        public static bool LineSegmentBoxIntersection(Vector2 p1, Vector2 p2, Vector2 r1, Vector2 r2, Vector2 r3, Vector2 r4, out Vector2 intersection) {
            intersection = Vector2.zero;
            bool isHitting = false;
            isHitting = LineSegmentsIntersection(p1, p2, r1, r2, out intersection);
            if (isHitting == false) isHitting = LineSegmentsIntersection(p1, p2, r2, r3, out intersection);
            if (isHitting == false) isHitting = LineSegmentsIntersection(p1, p2, r3, r4, out intersection);
            if (isHitting == false) isHitting = LineSegmentsIntersection(p1, p2, r4, r1, out intersection);

            return isHitting;
        }

        public static bool IntersectBox(Vector2 orig, Vector2 dir, Vector2 min, Vector2 max) {
            float tmin = (min.x - orig.x) / dir.x;
            float tmax = (max.x - orig.x) / dir.x;

            if (tmin > tmax) {
                var c = tmin;
                tmin = tmax;
                tmax = c;
            }

            float tymin = (min.y - orig.y) / dir.y;
            float tymax = (max.y - orig.y) / dir.y;

            if (tymin > tymax) {
                var c = tymin;
                tymin = tymax;
                tymax = c;
            }

            if ((tmin > tymax) || (tymin > tmax))
                return false;

            //if (tymin > tmin)
            //tmin = tymin;

            //if (tymax<tmax)
            //tmax = tymax;

            //float tzmin = (min.z - r.orig.z) / r.dir.z;
            //float tzmax = (max.z - r.orig.z) / r.dir.z;

            //if (tzmin > tzmax) swap(tzmin, tzmax);

            //if ((tmin > tzmax) || (tzmin > tmax))
            //return false;

            //if (tzmin > tmin)
            //tmin = tzmin;

            //if (tzmax<tmax)
            //tmax = tzmax;

            return true;
        }
    }
}
