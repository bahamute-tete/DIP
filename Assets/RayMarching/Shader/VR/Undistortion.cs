using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Pvr_UnitySDKConfigProfile
{
    public struct Lenses
    {
        public float separation;
        public float offset;
        public float distance;
        public int alignment;
        public const int AlignTop = -1;    // Offset is measured down from top of device.
        public const int AlignCenter = 0;  // Center alignment ignores offset, hence scale is zero.
        public const int AlignBottom = 1;  // Offset is measured up from bottom of device.

    }

    /// <summary>
    /// MaxFOV
    /// </summary>
    public struct MaxFOV
    {
        public float upper;
        public float lower;
        public float inner;
        public float outer;

    }

    public struct Distortion
    {
        public float k1;
        public float k2;
        public float k3;
        public float k4;
        public float k5;
        public float k6;
        public float distort(float r)
        {
            return 0;
        }
        public float distort(float r, float dist)
        {
            float r2 = r * dist * 1000.0f;
            float r3 = k1 * Mathf.Pow(r2, 5.0f) + k2 * Mathf.Pow(r2, 4.0f) + k3 * Mathf.Pow(r2, 3.0f) + k4 * Mathf.Pow(r2, 2.0f) + k5 * r2 + k6;
            return r3 / 1000.0f / dist; ;
        }

        public float diatortInv(float radious)
        {
            return 0;
        }

    }


    public struct Device
    {
        public Lenses devLenses;
        public MaxFOV devMaxFov;
        public Distortion devDistortion;
        public Distortion devDistortionInv;
    }

    public static readonly Device SimulateDevice = new Device
    {
        devLenses = { separation = 0.062f, offset = 0.0f, distance = 0.0403196f, alignment = Lenses.AlignCenter },
        devMaxFov = { upper = 40.0f, lower = 40.0f, inner = 40.0f, outer = 40.0f },
        devDistortion =
        {
            k1 = 2.333e-06f,
            k2 = -0.000126f,
            k3 = 0.002978f,
            k4 = -0.02615f,
            k5 = 1.089f,
            k6 = -0.0337f
        },
        devDistortionInv =
        {
            k1 = 1.342e-08f,
            k2 = 1.665e-06f,
            k3 = -0.0002797f,
            k4 = 0.001166f,
            k5 = 0.9945f,
            k6 = 0.004805f
        }
    };

    public Device device;
    public static readonly Pvr_UnitySDKConfigProfile Default = new Pvr_UnitySDKConfigProfile
    {
        device = SimulateDevice
    };

    public Pvr_UnitySDKConfigProfile Clone()
    {
        return new Pvr_UnitySDKConfigProfile
        {
            device = this.device
        };
    }
    public static Pvr_UnitySDKConfigProfile GetPicoProfile()
    {
        return new Pvr_UnitySDKConfigProfile { device = SimulateDevice };
    }

    public float[] GetLeftEyeVisibleTanAngles(float width, float height)
    {
        // Tan-angles from the max FOV.
        float fovLeft = (float)Mathf.Tan(-this.device.devMaxFov.outer * Mathf.PI / 180);
        float fovTop = (float)Mathf.Tan(this.device.devMaxFov.upper * Mathf.PI / 180);
        float fovRight = (float)Mathf.Tan(this.device.devMaxFov.inner * Mathf.PI / 180);
        float fovBottom = (float)Mathf.Tan(-this.device.devMaxFov.lower * Mathf.PI / 180);
        float halfWidth = width / 4;
        float halfHeight = height / 2;
        // Viewport center, measured from left lens position.
        float centerX = this.device.devLenses.separation / 2 - halfWidth;
        float centerY = 0.0f;
        float centerZ = this.device.devLenses.distance;
        // Tan-angles of the viewport edges, as seen through the lens.
        float screenLeft = this.device.devDistortion.distort((centerX - halfWidth) / centerZ, this.device.devLenses.distance);
        float screenTop = this.device.devDistortion.distort((centerY + halfHeight) / centerZ, this.device.devLenses.distance);
        float screenRight = this.device.devDistortion.distort((centerX + halfWidth) / centerZ, this.device.devLenses.distance);
        float screenBottom = this.device.devDistortion.distort((centerY - halfWidth) / centerZ, this.device.devLenses.distance);
        // Compare the two sets of tan-angles and take the value closer to zero on each side.
        float left = Mathf.Max(fovLeft, screenLeft);
        float top = Mathf.Min(fovTop, screenTop);
        float right = Mathf.Min(fovRight, screenRight);
        float bottom = Mathf.Max(fovBottom, screenBottom);
        return new float[] { left, top, right, bottom };
    }

    public float[] GetLeftEyeNoLensTanAngles(float width, float height)
    {
        // Tan-angles from the max FOV.
        float fovLeft = this.device.devDistortionInv.distort((float)Mathf.Tan(-this.device.devMaxFov.outer * Mathf.PI / 180), this.device.devLenses.distance);
        float fovTop = this.device.devDistortionInv.distort((float)Mathf.Tan(this.device.devMaxFov.upper * Mathf.PI / 180), this.device.devLenses.distance);
        float fovRight = this.device.devDistortionInv.distort((float)Mathf.Tan(this.device.devMaxFov.inner * Mathf.PI / 180), this.device.devLenses.distance);
        float fovBottom = this.device.devDistortionInv.distort((float)Mathf.Tan(-this.device.devMaxFov.lower * Mathf.PI / 180), this.device.devLenses.distance);
        // Viewport size.
        float halfWidth = width / 4;
        float halfHeight = height / 2;
        // Viewport center, measured from left lens position.
        float centerX = this.device.devLenses.separation / 2 - halfWidth;
        float centerY = 0.0f;
        float centerZ = this.device.devLenses.distance;
        // Tan-angles of the viewport edges, as seen through the lens.
        float screenLeft = (centerX - halfWidth) / centerZ;
        float screenTop = (centerY + halfHeight) / centerZ;
        float screenRight = (centerX + halfWidth) / centerZ;
        float screenBottom = (centerY - halfWidth) / centerZ;
        // Compare the two sets of tan-angles and take the value closer to zero on each side.
        float left = Mathf.Min(fovLeft, screenLeft);
        float top = Mathf.Min(fovTop, screenTop);
        float right = Mathf.Min(fovRight, screenRight);
        float bottom = Mathf.Max(fovBottom, screenBottom);
        return new float[] { left, top, right, bottom };
    }

    public Rect GetLeftEyeVisibleScreenRect(float[] undistortedFrustum, float width, float height)
    {

        float dist = this.device.devLenses.distance;
        float eyeX = (width - this.device.devLenses.separation) / 2;
        float eyeY = height / 2;
        float left = (undistortedFrustum[0] * dist + eyeX) / width;
        float top = (undistortedFrustum[1] * dist + eyeY) / height;
        float right = (undistortedFrustum[2] * dist + eyeX) / width;
        float bottom = (undistortedFrustum[3] * dist + eyeY) / height;
        return new Rect(left, bottom, right - left, top - bottom);
    }

}

public class Undistortion : MonoBehaviour
{

    Matrix4x4 realProj = Matrix4x4.identity;
    Pvr_UnitySDKConfigProfile pvr_UnitySDKConfigProfile = new Pvr_UnitySDKConfigProfile();


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void ModifyShadePara()
    {
        Matrix4x4 proj = Matrix4x4.identity;
        float near = GetComponent<Camera>().nearClipPlane;
        float far = GetComponent<Camera>().farClipPlane;
        float aspectFix = GetComponent<Camera>().rect.height / GetComponent<Camera>().rect.width / 2;

        proj[0, 0] *= aspectFix;
        Vector2 dir = transform.localPosition; // ignore Z
        dir = dir.normalized * 1.0f;
        proj[0, 2] *= Mathf.Abs(dir.x);
        proj[1, 2] *= Mathf.Abs(dir.y);
        proj[2, 2] = (near + far) / (near - far);
        proj[2, 3] = 2 * near * far / (near - far);

        Vector4 projvec = new Vector4(proj[0, 0], proj[1, 1],
                                    proj[0, 2] - 1, proj[1, 2] - 1) / 2;

        Vector4 unprojvec = new Vector4(realProj[0, 0], realProj[1, 1],
                                        realProj[0, 2] - 1, realProj[1, 2] - 1) / 2;

        float distortionFactor = 0.0241425f;
        Shader.SetGlobalVector("_Projection", projvec);
        Shader.SetGlobalVector("_Unprojection", unprojvec);
        Shader.SetGlobalVector("_Distortion1",
                                new Vector4(pvr_UnitySDKConfigProfile.device.devDistortion.k1, pvr_UnitySDKConfigProfile.device.devDistortion.k2, pvr_UnitySDKConfigProfile.device.devDistortion.k3, distortionFactor));
        Shader.SetGlobalVector("_Distortion2",
                               new Vector4(pvr_UnitySDKConfigProfile.device.devDistortion.k4, pvr_UnitySDKConfigProfile.device.devDistortion.k5, pvr_UnitySDKConfigProfile.device.devDistortion.k6));



    }
}
