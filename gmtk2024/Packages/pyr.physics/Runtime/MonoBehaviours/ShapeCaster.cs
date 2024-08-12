using System;
using System.Collections.Generic;
using System.Linq;
using pyr.Shared.Extensions;
using pyr.Shared.Types;
using SaintsField;
using Unity.Mathematics;
using UnityEngine;

namespace pyr.Physics;

[Icon(k_IconPath)]
[AddComponentMenu("Physics/ShapeCaster")]
[DisallowMultipleComponent]
public class ShapeCaster : MonoBehaviour
{
    [Serializable]
    public enum HitFilter
    {
        All,
        Nearest
    }

    [Serializable]
    public enum RaycastMode
    {
        Immediate,
        Job
    }

    private const string k_IconPath = "Packages/pyr.physics/Gizmos/d_ShapeCaster@64.png";

    [SerializeField] public bool Enabled = true;

    [InfoBox("Parameters.Origin & Parameters.Orientation are used as offset(s) to the GameObject's Transform")]
    [SerializeField]
    public Shape Shape;

    [SerializeField] public Raycast.Parameters Parameters;

    [SerializeField] public LayerMask LayerMask = -5;

    [ShowIf(nameof(IsUsingJob))] [SerializeField]
    public bool HitMultipleFaces;

    [ShowIf(nameof(IsUsingJob))] [SerializeField]
    public bool HitBackfaces;

    [SerializeField] public HitFilter Filter = HitFilter.Nearest;

    [Space(8)] [SerializeField] public RaycastMode Mode = RaycastMode.Job;

    public async void FixedUpdate()
    {
        if (!Enabled) return;

        var parameters = Parameters with
        {
            Origin = transform.position.AsFloat3() + Parameters.Origin,
            Orientation = math.mul(transform.rotation, Parameters.Orientation ?? quaternion.identity)
        };

        switch (Mode)
        {
            case RaycastMode.Immediate:
            {
                if (Filter == HitFilter.All)
                {
                    var hit = Raycast.Now.CastAll(Shape, parameters, LayerMask);
                    OnHit?.Invoke(hit.AsEnumerable());
                }
                else
                {
                    var hit = Raycast.Now.Cast(Shape, parameters, LayerMask);
                    OnHit?.Invoke(hit.AsEnumerable());
                }

                break;
            }
            case RaycastMode.Job:
            {
                if (Filter == HitFilter.All)
                {
                    var hit = await Raycast.CastAll(Shape, parameters,
                        QueryParameters.Default with
                        {
                            layerMask = LayerMask, hitBackfaces = HitBackfaces, hitMultipleFaces = HitMultipleFaces
                        });
                    OnHit?.Invoke(hit.AsEnumerable());
                }
                else
                {
                    var hit = await Raycast.Cast(Shape, parameters,
                        QueryParameters.Default with
                        {
                            layerMask = LayerMask, hitBackfaces = HitBackfaces, hitMultipleFaces = HitMultipleFaces
                        });
                    OnHit?.Invoke(hit.AsEnumerable());
                }

                break;
            }
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public bool IsUsingJob()
    {
        return Mode == RaycastMode.Job;
    }

    public event Action<IEnumerable<RaycastHit>>? OnHit;
}
