using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Unity.Adjust.Editor
{
    /// <summary>
    /// Cửa sổ cấu hình Adjust trong Unity Editor:
    /// - Quản lý AppToken và Môi trường (Production / Sandbox)
    /// - Soạn thảo và xuất file cấu hình Assets/Resources/Info.json
    /// </summary>
    public class AdjustEditorWindow : EditorWindow
    {
        private string _appToken = "";
        private bool _isProduction = true;
        private string _targetExportPath = "Assets/Resources/Info.json";
        private Vector2 _scrollPos;

        [MenuItem("Unity Core/Adjust/Setup & Configuration", false, 18)]
        public static void ShowWindow()
        {
            var window = GetWindow<AdjustEditorWindow>("Adjust Setup");
            window.minSize = new Vector2(480, 400);
            window.Show();
        }

        private void OnEnable()
        {
            LoadCurrentSettings();
        }

        private void LoadCurrentSettings()
        {
            try
            {
                string fullPath = Path.Combine(Application.dataPath, "..", _targetExportPath);
                if (File.Exists(fullPath))
                {
                    string json = File.ReadAllText(fullPath);
                    _appToken = ExtractValue(json, "AdjustToken", "AdjustAppToken", "AppToken");
                    string env = ExtractValue(json, "AdjustEnvironment", "Environment");
                    if (!string.IsNullOrEmpty(env) && env.Equals("Sandbox", StringComparison.OrdinalIgnoreCase))
                    {
                        _isProduction = false;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[AdjustEditorWindow] Lỗi đọc cấu hình: {ex.Message}");
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            GUILayout.Label("Unity Adjust - Configuration", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Cấu hình App Token và Môi trường phân tích Attribution & Ad Revenue từ Adjust.", MessageType.Info);

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            EditorGUILayout.Space(6);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("1. Cấu Hình Tài Khoản Adjust", EditorStyles.boldLabel);

            _appToken = EditorGUILayout.TextField("Adjust App Token:", _appToken);
            _isProduction = EditorGUILayout.Toggle("Production Environment:", _isProduction);
            if (!_isProduction)
            {
                EditorGUILayout.HelpBox("Đang ở chế độ Sandbox (dành cho môi trường Test & Debug).", MessageType.Warning);
            }

            EditorGUILayout.Space(4);
            _targetExportPath = EditorGUILayout.TextField("Export File:", _targetExportPath);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Info.json (Mặc định)")) _targetExportPath = "Assets/Resources/Info.json";
            if (GUILayout.Button("Info_Android.json")) _targetExportPath = "Assets/Resources/Info_Android.json";
            if (GUILayout.Button("Info_iOS.json")) _targetExportPath = "Assets/Resources/Info_iOS.json";
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(6);
            if (GUILayout.Button("💾 Lưu Cấu Hình Vào Resources", GUILayout.Height(36)))
            {
                SaveConfigToResources();
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndScrollView();
        }

        private void SaveConfigToResources()
        {
            try
            {
                string fullPath = Path.Combine(Application.dataPath, "..", _targetExportPath);
                string dir = Path.GetDirectoryName(fullPath);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                string existing = File.Exists(fullPath) ? File.ReadAllText(fullPath) : "{\n}";
                string updated = UpsertJsonKey(existing, "AdjustToken", _appToken);
                updated = UpsertJsonKey(updated, "AdjustEnvironment", _isProduction ? "Production" : "Sandbox");

                File.WriteAllText(fullPath, updated);
                AssetDatabase.Refresh();
                EditorUtility.DisplayDialog("Thành công", $"Đã lưu cấu hình Adjust vào: {_targetExportPath}", "OK");
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog("Lỗi", $"Lỗi ghi file cấu hình: {ex.Message}", "OK");
            }
        }

        private string UpsertJsonKey(string json, string key, string value)
        {
            string searchKey = $"\"{key}\"";
            int idx = json.IndexOf(searchKey, StringComparison.OrdinalIgnoreCase);
            if (idx >= 0)
            {
                int colon = json.IndexOf(':', idx + searchKey.Length);
                int q1 = json.IndexOf('"', colon + 1);
                int q2 = json.IndexOf('"', q1 + 1);
                if (q1 >= 0 && q2 > q1)
                {
                    return json.Substring(0, q1 + 1) + value + json.Substring(q2);
                }
            }
            else
            {
                int lastBrace = json.LastIndexOf('}');
                if (lastBrace >= 0)
                {
                    string toInsert = $"  \"{key}\": \"{value}\",\n";
                    return json.Insert(lastBrace, toInsert);
                }
            }
            return json;
        }

        private string ExtractValue(string json, params string[] keys)
        {
            if (string.IsNullOrEmpty(json)) return "";
            foreach (var key in keys)
            {
                string searchKey = $"\"{key}\"";
                int idx = json.IndexOf(searchKey, StringComparison.OrdinalIgnoreCase);
                if (idx >= 0)
                {
                    int colon = json.IndexOf(':', idx + searchKey.Length);
                    if (colon >= 0)
                    {
                        int q1 = json.IndexOf('"', colon + 1);
                        if (q1 >= 0)
                        {
                            int q2 = json.IndexOf('"', q1 + 1);
                            if (q2 > q1)
                            {
                                return json.Substring(q1 + 1, q2 - q1 - 1);
                            }
                        }
                    }
                }
            }
            return "";
        }
    }
}
