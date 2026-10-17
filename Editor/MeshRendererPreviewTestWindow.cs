using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using nadena.dev.ndmf.preview;
using UnityEditor;
using UnityEngine;

namespace nadena.dev.ndmf
{
    internal sealed class MeshRendererPreviewTestWindow : EditorWindow
    {
        private MeshRenderer _renderer;
        private PreviewSession _session;

        [MenuItem("Tools/NDM Framework/Debug Tools/MeshRenderer Preview Test")]
        private static void Open()
        {
            GetWindow<MeshRendererPreviewTestWindow>("MeshRenderer Preview Test");
        }

        private void OnGUI()
        {
            EditorGUILayout.HelpBox(
                "指定したMeshRendererだけを、加工しないフィルタ1段でプレビューします。" +
                "開始時にアクティブなSceneビューへ適用し、他のプラグインの加工は含めません。",
                MessageType.Info
            );

            using (new EditorGUI.DisabledScope(_session != null))
            {
                _renderer = (MeshRenderer)EditorGUILayout.ObjectField(
                    "対象MeshRenderer", _renderer, typeof(MeshRenderer), true
                );
            }

            if (_session == null)
            {
                using (new EditorGUI.DisabledScope(_renderer == null || SceneView.lastActiveSceneView == null))
                {
                    if (GUILayout.Button("プレビュー開始"))
                    {
                        StartPreview();
                    }
                }
            }
            else if (GUILayout.Button("プレビュー停止"))
            {
                StopPreview();
            }
        }

        private void StartPreview()
        {
            _session = new PreviewSession();
            _session.AddMutator(new SequencePoint(), new PassthroughFilter(_renderer));
            _session.OverrideCamera(SceneView.lastActiveSceneView.camera);
            SceneView.RepaintAll();
        }

        private void OnDisable()
        {
            StopPreview();
        }

        private void StopPreview()
        {
            _session?.Dispose();
            _session = null;
            SceneView.RepaintAll();
        }

        private sealed class PassthroughFilter : IRenderFilter
        {
            private readonly MeshRenderer _renderer;

            internal PassthroughFilter(MeshRenderer renderer)
            {
                _renderer = renderer;
            }

            public ImmutableList<RenderGroup> GetTargetGroups(ComputeContext context)
            {
                return _renderer != null
                    ? ImmutableList.Create(RenderGroup.For(_renderer))
                    : ImmutableList<RenderGroup>.Empty;
            }

            public Task<IRenderFilterNode> Instantiate(
                RenderGroup group,
                IEnumerable<(Renderer, Renderer)> proxyPairs,
                ComputeContext context
            )
            {
                return Task.FromResult<IRenderFilterNode>(new PassthroughNode());
            }
        }

        private sealed class PassthroughNode : IRenderFilterNode
        {
            public RenderAspects WhatChanged => 0;
        }
    }
}
