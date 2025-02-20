using UnityEngine;

namespace Math
{
    public static class InverseKinematics
    {
        public static void TwoBoneIK(Transform[] bones, Vector3[] boneAngleOffsets, Vector3 vTarget, Vector3 vPole)
        {
            if (bones == null || 
                bones.Length != 3 ||
                boneAngleOffsets == null || 
                boneAngleOffsets.Length != 3)
            {
                return;
            }

            Vector3 vTowardPole = vPole - bones[0].position;
            Vector3 vTowardTarget = vTarget - bones[0].position;

            float fRootBoneLength = Vector3.Distance(bones[0].position, bones[1].position);
            float fSecondBoneLength = Vector3.Distance(bones[1].position, bones[2].position);
            float fTotalChainLength = fRootBoneLength + fSecondBoneLength;

            // Align root with target
            bones[0].rotation = Quaternion.LookRotation(vTowardTarget, vTowardPole);
            bones[0].localRotation *= Quaternion.Euler(boneAngleOffsets[0]);

            Vector3 vTowardSecondBone = bones[1].position - bones[0].position;

            float fTargetDistance = Vector3.Distance(bones[0].position, vTarget);

            // Limit hypotenuse to under the total bone distance to prevent invalid triangles
            fTargetDistance = Mathf.Min(fTargetDistance, fTotalChainLength * 0.9999f);

            // Solve for the angle for the root bone
            // See https://en.wikipedia.org/wiki/Law_of_cosines
            float fAdjacent = ((fRootBoneLength * fRootBoneLength) + (fTargetDistance * fTargetDistance) -
                                (fSecondBoneLength * fSecondBoneLength)) /
                                (2 * fTargetDistance * fRootBoneLength);

            float fAngle = Mathf.Acos(fAdjacent) * Mathf.Rad2Deg;

            // We rotate around the vector orthogonal to both pole and second bone
            Vector3 vCross = Vector3.Cross(vTowardPole, vTowardSecondBone);
            if (!float.IsNaN(fAngle))
            {
                bones[0].RotateAround(bones[0].position, vCross, -fAngle);
            }

            // We've rotated the root bone to the right place, so we just 
            // look at the target from the elbow to get the final rotation
            Quaternion qSecondBoneTargetRotation = Quaternion.LookRotation(vTarget - bones[1].position, vCross);
            qSecondBoneTargetRotation *= Quaternion.Euler(boneAngleOffsets[1]);
            bones[1].rotation = qSecondBoneTargetRotation;
        }
    }
}