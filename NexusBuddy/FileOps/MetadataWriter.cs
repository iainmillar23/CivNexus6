﻿using Firaxis.Granny;
using System.IO;
using NexusBuddy.GrannyWrappers;
using System.Globalization;
using System.Collections.Generic;
using NexusBuddy.GrannyInfos;


namespace NexusBuddy.FileOps
{
    class MetadataWriter
    {
        public static void WriteGeoAnimFile(IGrannyFile file, int currentModelIndex, string className)
        {
            string fgxFilename = Path.GetFileName(file.Filename);
            string filenameNoExt = Path.GetFileNameWithoutExtension(file.Filename);

            string directory = Path.GetDirectoryName(file.Filename);

            string numberFormat = "f6";

            bool isAnimation = false;
            string instanceName = "Geometry";
            string fileExtension = ".geo";

            if (file.Animations.Count > 0 && file.Models.Count == 0)
            {
                isAnimation = true;
                instanceName = "Animation";
                fileExtension = ".anm";
            }

            string geoFilename = filenameNoExt + fileExtension;

            string wigFilename = "";
            if (className.Equals("Leader") && instanceName.Equals("Geometry"))
            {
                wigFilename = filenameNoExt + ".wig";
                string wigFullPath = directory + "\\" + wigFilename;
                File.Copy(CivNexusSixApplicationForm.form.dummyWigFilename, wigFullPath, true);
            }

            StreamWriter outputWriter = new StreamWriter(new FileStream(directory + "\\" + geoFilename, FileMode.Create));

            WriteAssetHeader(instanceName, outputWriter);

            WriteBlankCookParams(outputWriter);
            WriteVersion(outputWriter);

            if (isAnimation)
            {
                outputWriter.WriteLine("<m_fDuration>" + file.Animations[0].Duration.ToString(numberFormat, CultureInfo.InvariantCulture) + "</m_fDuration>");
            }
            else
            {
                outputWriter.WriteLine("<m_Meshes>");
                foreach (IGrannyMesh mesh in file.Meshes)
                {
                    GrannyMeshWrapper meshWrapper = new GrannyMeshWrapper(mesh);

                    outputWriter.WriteLine("<Element>");
                    outputWriter.WriteLine("<m_Name text=\"" + mesh.Name + "\"/>");

                    outputWriter.WriteLine("<m_Groups>");

                    int totalTriangles = 0;
                    foreach (IGrannyTriMaterialGroup triMaterialGroup in mesh.TriangleMaterialGroups)
                    {
                        outputWriter.WriteLine("<Element>");
                        outputWriter.WriteLine("<m_Name text=\"" + mesh.MaterialBindings[triMaterialGroup.MaterialIndex].Name + "\"/>");
                        outputWriter.WriteLine("<m_nFirstPrim>" + triMaterialGroup.TriFirst + "</m_nFirstPrim>");
                        outputWriter.WriteLine("<m_nPrims>" + triMaterialGroup.TriCount + "</m_nPrims>");
                        outputWriter.WriteLine("</Element>");

                        totalTriangles += triMaterialGroup.TriCount;
                    }
                    outputWriter.WriteLine("</m_Groups>");

                    outputWriter.WriteLine("<m_nBoundBoneCount>" + mesh.BoneBindings.Count + "</m_nBoundBoneCount>");
                    outputWriter.WriteLine("<m_nPrimitiveCount>" + totalTriangles + "</m_nPrimitiveCount>");
                    outputWriter.WriteLine("<m_nVertexCount>" + mesh.VertexCount + "</m_nVertexCount>");

                    outputWriter.WriteLine("</Element>");
                }
                outputWriter.WriteLine("</m_Meshes>");

                outputWriter.WriteLine("<m_Bones>");
                foreach (IGrannyBone bone in file.Models[currentModelIndex].Skeleton.Bones)
                {
                    outputWriter.WriteLine("<Element text=\"" + bone.Name + "\"/>");
                }
                outputWriter.WriteLine("</m_Bones>");
                outputWriter.WriteLine("<m_ModelName text=\"" + file.Models[currentModelIndex].Name + "\"/>");
            }

            WriteSourcePathAndTimes(outputWriter);

            WriteClassName(className, outputWriter);

            outputWriter.WriteLine("<m_DataFiles>");
            outputWriter.WriteLine("<Element>");
            outputWriter.WriteLine("<m_ID text=\"GR2\"/>");
            outputWriter.WriteLine("<m_RelativePath text=\"" + fgxFilename + "\"/>");
            outputWriter.WriteLine("</Element>");

            if (className.Equals("Leader") && instanceName.Equals("Geometry"))
            {
                outputWriter.WriteLine("<Element>");
                outputWriter.WriteLine("<m_ID text=\"WIG\"/>");
                outputWriter.WriteLine("<m_RelativePath text=\"" + wigFilename + "\"/>");
                outputWriter.WriteLine("</Element>");
            }

            outputWriter.WriteLine("</m_DataFiles>");

            WriteFooter(className, filenameNoExt, instanceName, outputWriter);

            outputWriter.Close();
        }

        public static void WriteTextureFile(string outputDirectory, string filenameNoExt, Dictionary<string, string> imageMetadataDict, string className, TextureClass textureClass)
        {
            string numberFormat = "f6";

            string instanceName = "Texture";
            string fileExtension = ".tex";
            string texFilename = filenameNoExt + fileExtension;

            StreamWriter outputWriter = new StreamWriter(new FileStream(outputDirectory + "\\" + texFilename, FileMode.Create));

            WriteAssetHeader("Texture", outputWriter);

            outputWriter.WriteLine("<m_ExportSettings>");
            outputWriter.WriteLine("<ePixelformat>PF_" + textureClass.PixelFormat + "</ePixelformat>");
            outputWriter.WriteLine("<eFilterType>FT_" + textureClass.MipFilter + "</eFilterType>");
            outputWriter.WriteLine("<bUseMips>" + textureClass.AllowArtistMips.ToString().ToLower() + "</bUseMips>");
            outputWriter.WriteLine("<iNumManualMips>0</iNumManualMips>");
            outputWriter.WriteLine("<bCompleteMipChain>" + textureClass.AllowArtistMips.ToString().ToLower() + "</bCompleteMipChain>");
            outputWriter.WriteLine("<fValueClampMin>" + textureClass.ExportClampMin.ToString(numberFormat, CultureInfo.InvariantCulture) + "</fValueClampMin>");
            outputWriter.WriteLine("<fValueClampMax>" + textureClass.ExportClampMax.ToString(numberFormat, CultureInfo.InvariantCulture) + "</fValueClampMax>");
            outputWriter.WriteLine("<fSupportScale>" + textureClass.MipSupportScale.ToString(numberFormat, CultureInfo.InvariantCulture) + "</fSupportScale>");
            outputWriter.WriteLine("<fGammaIn>" + textureClass.ExportGammaIn.ToString(numberFormat, CultureInfo.InvariantCulture) + "</fGammaIn>");
            outputWriter.WriteLine("<fGammaOut>" + textureClass.ExportGammaOut.ToString(numberFormat, CultureInfo.InvariantCulture) + "</fGammaOut>");
            outputWriter.WriteLine("<iSlabWidth>0</iSlabWidth>");
            outputWriter.WriteLine("<iSlabHeight>0</iSlabHeight>");
            outputWriter.WriteLine("<iColorKeyX>64</iColorKeyX>");
            outputWriter.WriteLine("<iColorKeyY>64</iColorKeyY>");
            outputWriter.WriteLine("<iColorKeyZ>64</iColorKeyZ>");
            outputWriter.WriteLine("<eExportMode>TEXTURE_2D</eExportMode>");
            outputWriter.WriteLine("<bSampleFromTopLayer>false</bSampleFromTopLayer>");
            outputWriter.WriteLine("</m_ExportSettings>");

            WriteCookParams(outputWriter, textureClass.CookParams);
            WriteVersion(outputWriter);

            outputWriter.WriteLine("<m_Height>" + imageMetadataDict["height"] + "</m_Height>");
            outputWriter.WriteLine("<m_Width>" + imageMetadataDict["width"] + "</m_Width>");
            outputWriter.WriteLine("<m_Depth>" + imageMetadataDict["depth"] + "</m_Depth>");
            outputWriter.WriteLine("<m_NumMipMaps>" + imageMetadataDict["mipLevels"] + "</m_NumMipMaps>");

            WriteSourcePathAndTimes(outputWriter);

            WriteClassName(className, outputWriter);

            outputWriter.WriteLine("<m_DataFiles>");
            outputWriter.WriteLine("<Element>");
            outputWriter.WriteLine("<m_ID text=\"DDS\"/>");
            outputWriter.WriteLine("<m_RelativePath text=\"" + filenameNoExt + ".dds\"/>");
            outputWriter.WriteLine("</Element>");
            outputWriter.WriteLine("</m_DataFiles>");

            WriteFooter(className, filenameNoExt, instanceName, outputWriter);

            outputWriter.Close();
        }

        private static void WriteSourcePathAndTimes(StreamWriter outputWriter)
        {
            outputWriter.WriteLine("<m_SourceFilePath text=\"\"/>");
            outputWriter.WriteLine("<m_SourceObjectName text=\"\"/>");
            outputWriter.WriteLine("<m_ImportedTime>0</m_ImportedTime>");
            outputWriter.WriteLine("<m_ExportedTime>0</m_ExportedTime>");
        }

        private static void WriteFooter(string className, string assetName, string instanceName, StreamWriter outputWriter)
        {
            outputWriter.WriteLine("<m_Name text=\"" + assetName + "\"/>");
            outputWriter.WriteLine("<m_Description text=\"" + assetName + "\"/>");

            outputWriter.WriteLine("<m_Tags>");
            outputWriter.WriteLine("<Element text=\"" + className + "\"/>");
            outputWriter.WriteLine("</m_Tags>");

            outputWriter.WriteLine("<m_Groups/>");
            outputWriter.WriteLine("</AssetObjects:" + instanceName + "Instance>");
        }

        private static void WriteClassName(string className, StreamWriter outputWriter)
        {
            outputWriter.WriteLine("<m_ClassName text=\"" + className + "\"/>");
        }

        private static void WriteBlankCookParams(StreamWriter outputWriter)
        {
            outputWriter.WriteLine("<m_CookParams>");
            outputWriter.WriteLine("<m_Values/>");
            outputWriter.WriteLine("</m_CookParams>");
        }

        private static void WriteCookParams(StreamWriter outputWriter, List<CookParam> cookParams)
        {
            outputWriter.WriteLine("<m_CookParams>");
            outputWriter.WriteLine("<m_Values>");
            foreach (CookParam cookParam in cookParams)
            {
                string paramType = "";
                string paramTypeLetter = "";
                if (cookParam.defaultVal.Contains("."))
                {
                    paramType = "Float";
                    paramTypeLetter = "f";
                }
                else if (cookParam.defaultVal.Equals("true") || cookParam.defaultVal.Equals("false"))
                {
                    paramType = "Bool";
                    paramTypeLetter = "b";
                }
                else
                {
                    paramType = "Int";
                    paramTypeLetter = "n";
                }

                outputWriter.WriteLine("<Element class=\"AssetObjects.." + paramType + "Value\">");
                outputWriter.WriteLine("<m_" + paramTypeLetter + "Value>" + cookParam.defaultVal + "</m_" + paramTypeLetter + "Value>");
                outputWriter.WriteLine("<m_ParamName text=\"" + cookParam.name + "\"/>");
                outputWriter.WriteLine("</Element>");
            }

            outputWriter.WriteLine("</m_Values>");
            outputWriter.WriteLine("</m_CookParams>");
        }

        private static void WriteVersion(StreamWriter outputWriter)
        {
            outputWriter.WriteLine("<m_Version>");
            outputWriter.WriteLine("<major>0</major>");
            outputWriter.WriteLine("<minor>0</minor>");
            outputWriter.WriteLine("<build>0</build>");
            outputWriter.WriteLine("<revision>0</revision>");
            outputWriter.WriteLine("</m_Version>");
        }

        private static void WriteAssetHeader(string instanceName, StreamWriter outputWriter)
        {
            outputWriter.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\" ?>");
            outputWriter.WriteLine("<AssetObjects:" + instanceName + "Instance>");
        }


        public static void WriteMultiModelAssetFile(string assetsDirectory, string assetName, List<IGrannyFile> files, Dictionary<string, string> civ6ShortNameToLongNameLookup, string className, string dsgName, string prettyAssetFilename, List<Dictionary<string, string>> materialBindingToMtlDict)
        {
            string filenameNoExt = Path.GetFileNameWithoutExtension(assetName);
            if (prettyAssetFilename != null && prettyAssetFilename.Length > 0)
            {
                filenameNoExt = prettyAssetFilename;
            }
            //string directory = Path.GetDirectoryName(assetName);
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            string assetFilename = textInfo.ToTitleCase(filenameNoExt) + ".ast";
            string instanceName = "Asset";

            StreamWriter outputWriter = new StreamWriter(new FileStream(assetsDirectory + "\\" + assetFilename, FileMode.Create));
            WriteAssetHeader(instanceName, outputWriter);
            WriteBehaviorMetadataToStream(civ6ShortNameToLongNameLookup, dsgName, outputWriter);

            WriteGeometrySetMetadataToStream(files, outputWriter, materialBindingToMtlDict, className);

            WriteBlankCookParams(outputWriter);
            WriteVersion(outputWriter);

            outputWriter.WriteLine("<m_ParticleEffects/>");
            outputWriter.WriteLine("<m_Geometries/>");
            outputWriter.WriteLine("<m_Animations/>");
            outputWriter.WriteLine("<m_Materials/>");

            WriteClassName(className, outputWriter);

            outputWriter.WriteLine("<m_DataFiles/>");

            WriteFooter(className, textInfo.ToTitleCase(filenameNoExt), instanceName, outputWriter);

            outputWriter.Close();
        }


        public static void WriteAssetFile(IGrannyFile file, Dictionary<string, string> civ6ShortNameToLongNameLookup, string className, string dsgName, string prettyAssetFilename, List<Dictionary<string, string>> materialBindingToMtlDicts)
        {
            string filenameNoExt = Path.GetFileNameWithoutExtension(file.Filename);
            if (prettyAssetFilename != null && prettyAssetFilename.Length > 0)
            {
                filenameNoExt = prettyAssetFilename;
            }
            string directory = Path.GetDirectoryName(file.Filename);
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            string assetFilename = textInfo.ToTitleCase(filenameNoExt) + ".ast";
            string instanceName = "Asset";

            StreamWriter outputWriter = new StreamWriter(new FileStream(directory + "\\" + assetFilename, FileMode.Create));
            WriteAssetHeader(instanceName, outputWriter);
            WriteBehaviorMetadataToStream(civ6ShortNameToLongNameLookup, dsgName, outputWriter);
            WriteGeometrySetMetadataToStream(new List<IGrannyFile>{file}, outputWriter, materialBindingToMtlDicts, className);
            WriteBlankCookParams(outputWriter);
            WriteVersion(outputWriter);

            outputWriter.WriteLine("<m_ParticleEffects/>");
            outputWriter.WriteLine("<m_Geometries/>");
            outputWriter.WriteLine("<m_Animations/>");
            outputWriter.WriteLine("<m_Materials/>");

            WriteClassName(className, outputWriter);

            outputWriter.WriteLine("<m_DataFiles/>");

            WriteFooter(className, textInfo.ToTitleCase(filenameNoExt), instanceName, outputWriter);

            outputWriter.Close();
        }

        public static void WriteMaterialFile(string directory, string materialFilename, string baseTextureMap, string materialClass, bool tintMask)
        {
            string tintMaskMap = "";
            if (tintMask)
            {
                tintMaskMap = baseTextureMap.ToLower().Replace("diff", "diff_t");
            }

            if (!materialFilename.Contains("ColorMap") && !materialFilename.Contains("Generic_Grey_8"))
            {
                StreamWriter outputWriter = new StreamWriter(new FileStream(directory + "\\" + materialFilename, FileMode.Create));
                outputWriter.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\" ?>");
                outputWriter.WriteLine("<AssetObjects..MaterialInstance>");
                outputWriter.WriteLine("<m_CookParams>");
                outputWriter.WriteLine("<m_Values>");

                string normMap = "Flat_Normal";
                string metalMap = "Black_Metalness";
                string glossMap = "Black_Gloss";
                string aoMap = "White_AO";

                if (materialClass.StartsWith("Leader"))
                {
                    string blurMap = "";
                    string opacMap = "";
                    string tangMap = "";

                    normMap = baseTextureMap.ToLower().Replace("diff", "norm");
                    glossMap = baseTextureMap.ToLower().Replace("diff", "sref_g");
                    metalMap = baseTextureMap.ToLower().Replace("diff", "sref_m");

                    if (baseTextureMap.Contains("hair"))
                    {
                        opacMap = baseTextureMap.ToLower().Replace("diff", "opac");
                        tangMap = baseTextureMap.ToLower().Replace("diff", "tang");
                    }

                    if (baseTextureMap.Contains("skin"))
                    {
                        blurMap = baseTextureMap.ToLower().Replace("diff", "blur");
                    }

                    baseTextureMap = baseTextureMap.ToLower().Replace("diff", "diff_sref_a");

                    string xml_chunk = @"<Element class=""AssetObjects..ObjectValue"">
                                            <m_ObjectName text=""" + blurMap + @"""/>
                                            <m_eObjectType>TEXTURE</m_eObjectType>
                                            <m_ParamName text=""BlurWidth""/>
                                        </Element>
                                        <Element class=""AssetObjects..ObjectValue"">
                                            <m_ObjectName text=""" + opacMap + @"""/>
                                            <m_eObjectType>TEXTURE</m_eObjectType>
                                            <m_ParamName text=""Opacity""/>
                                        </Element>
                                        <Element class=""AssetObjects..ObjectValue"">
                                            <m_ObjectName text=""""/>
                                            <m_eObjectType>TEXTURE</m_eObjectType>
                                            <m_ParamName text=""SpecTint""/>
                                        </Element>
                                        <Element class=""AssetObjects..ObjectValue"">
                                            <m_ObjectName text=""""/>
                                            <m_eObjectType>TEXTURE</m_eObjectType>
                                            <m_ParamName text=""Anisotropy""/>
                                        </Element>
                                        <Element class=""AssetObjects..ObjectValue"">
                                            <m_ObjectName text=""" + tangMap + @"""/>
                                            <m_eObjectType>TEXTURE</m_eObjectType>
                                            <m_ParamName text=""Tangent""/>
                                        </Element>
                                        <Element class=""AssetObjects..ObjectValue"">
                                            <m_ObjectName text=""""/>
                                            <m_eObjectType>TEXTURE</m_eObjectType>
                                            <m_ParamName text=""Fuzz""/>
                                        </Element>
                                        <Element class=""AssetObjects..ObjectValue"">
                                            <m_ObjectName text=""""/>
                                            <m_eObjectType>TEXTURE</m_eObjectType>
                                            <m_ParamName text=""FuzzTint""/>
                                        </Element>";

                    outputWriter.Write(xml_chunk);
                }

                outputWriter.WriteLine("<Element class=\"AssetObjects..ObjectValue\">");
                outputWriter.WriteLine("<m_ObjectName text=\"" + baseTextureMap.ToLower() + "\"/>");
                outputWriter.WriteLine("<m_eObjectType>TEXTURE</m_eObjectType>");
                outputWriter.WriteLine("<m_ParamName text=\"BaseColor\"/>");
                outputWriter.WriteLine("</Element>");

                bool isDecal = materialClass.Equals("DecalMaterial");
                if (isDecal)
                {
                    outputWriter.WriteLine("<Element class=\"AssetObjects..ObjectValue\">");
                    outputWriter.WriteLine("<m_ObjectName text=\"\"/>");
                    outputWriter.WriteLine("<m_eObjectType>TEXTURE</m_eObjectType>");
                    outputWriter.WriteLine("<m_ParamName text=\"Heightmap\"/>");
                    outputWriter.WriteLine("</Element>");
                    outputWriter.WriteLine("<Element class=\"AssetObjects..ObjectValue\">");
                    outputWriter.WriteLine("<m_ObjectName text=\"\"/>");
                    outputWriter.WriteLine("<m_eObjectType>TEXTURE</m_eObjectType>");
                    outputWriter.WriteLine("<m_ParamName text=\"Spec\"/>");
                    outputWriter.WriteLine("</Element>");
                    outputWriter.WriteLine("<Element class=\"AssetObjects..ObjectValue\">");
                    outputWriter.WriteLine("<m_ObjectName text=\"\"/>");
                    outputWriter.WriteLine("<m_eObjectType>TEXTURE</m_eObjectType>");
                    outputWriter.WriteLine("<m_ParamName text=\"FOWColor\"/>");
                    outputWriter.WriteLine("</Element>");
                }
                else { 
                    outputWriter.WriteLine("<Element class=\"AssetObjects..ObjectValue\">");
                    outputWriter.WriteLine("<m_ObjectName text=\"" + normMap + "\" />");
                    outputWriter.WriteLine("<m_eObjectType>TEXTURE</m_eObjectType>");
                    outputWriter.WriteLine("<m_ParamName text=\"Normal\"/>");
                    outputWriter.WriteLine("</Element>");
                    outputWriter.WriteLine("<Element class=\"AssetObjects..ObjectValue\">");
                    outputWriter.WriteLine("<m_ObjectName text=\"" + aoMap + "\"/>");
                    outputWriter.WriteLine("<m_eObjectType>TEXTURE</m_eObjectType>");
                    outputWriter.WriteLine("<m_ParamName text=\"AO\"/>");
                    outputWriter.WriteLine("</Element>");
                    outputWriter.WriteLine("<Element class=\"AssetObjects..BoolValue\">");
                    outputWriter.WriteLine("<m_bValue>false</m_bValue>");
                    outputWriter.WriteLine("<m_ParamName text=\"Seperate_AO_UV\"/>");
                    outputWriter.WriteLine("</Element>");
                    outputWriter.WriteLine("<Element class=\"AssetObjects..ObjectValue\">");
                    outputWriter.WriteLine("<m_ObjectName text=\"" + metalMap + "\"/>");
                    outputWriter.WriteLine("<m_eObjectType>TEXTURE</m_eObjectType>");
                    outputWriter.WriteLine("<m_ParamName text=\"Metalness\"/>");
                    outputWriter.WriteLine("</Element>");
                    outputWriter.WriteLine("<Element class=\"AssetObjects..ObjectValue\">");
                    outputWriter.WriteLine("<m_ObjectName text=\""+ tintMaskMap + "\"/>");
                    outputWriter.WriteLine("<m_eObjectType>TEXTURE</m_eObjectType>");
                    outputWriter.WriteLine("<m_ParamName text=\"TintMask\"/>");
                    outputWriter.WriteLine("</Element>");
                    outputWriter.WriteLine("<Element class=\"AssetObjects..ObjectValue\">");
                    outputWriter.WriteLine("<m_ObjectName text=\"" + glossMap + "\"/>");
                    outputWriter.WriteLine("<m_eObjectType>TEXTURE</m_eObjectType>");
                    outputWriter.WriteLine("<m_ParamName text=\"Gloss\"/>");
                    outputWriter.WriteLine("</Element>");

                    if (materialClass.StartsWith("Leader"))
                    {
                        string red = "150";
                        string green = "150";
                        string blue = "150";

                        if (baseTextureMap.Contains("skin"))
                        {
                            red = "165";
                            green = "40";
                            blue = "28";
                        } 

                        string xml_chunk = @"<Element class=""AssetObjects..ObjectValue"">
                                            <m_ObjectName text=""""/>
                                            <m_eObjectType>TEXTURE</m_eObjectType>
                                            <m_ParamName text=""Translucency""/>
                                        </Element>
                                        <Element class=""AssetObjects..RGBValue"">
                                            <m_r>" + red + @"</m_r>
                                            <m_g>" + green + @"</m_g>
                                            <m_b>" + blue + @"</m_b>
                                            <m_ParamName text=""TranslucencyColor""/>
                                        </Element>
                                        <Element class=""AssetObjects..BoolValue"">
                                            <m_bValue>false</m_bValue>
                                            <m_ParamName text=""ForceTransparency""/>
                                        </Element>";
                        outputWriter.Write(xml_chunk);
                    } else
                    {
                        outputWriter.WriteLine("<Element class=\"AssetObjects..BoolValue\">");
                        outputWriter.WriteLine("<m_bValue>false</m_bValue>");
                        outputWriter.WriteLine("<m_ParamName text=\"Translucency\"/>");
                        outputWriter.WriteLine("</Element>");
                    }
                }

                if (materialClass.Equals("Landmark"))
                {
                    outputWriter.WriteLine("<Element class=\"AssetObjects..ObjectValue\">");
                    outputWriter.WriteLine("<m_ObjectName text=\"\"/>");
                    outputWriter.WriteLine("<m_eObjectType>TEXTURE</m_eObjectType>");
                    outputWriter.WriteLine("<m_ParamName text=\"Opacity\"/>");
                    outputWriter.WriteLine("</Element>");
                }

                outputWriter.WriteLine("</m_Values>");
                outputWriter.WriteLine("</m_CookParams>");
                outputWriter.WriteLine("<m_Version>");
                outputWriter.WriteLine("<major>3</major>");
                outputWriter.WriteLine("<minor>0</minor>");
                outputWriter.WriteLine("<build>196</build>");
                outputWriter.WriteLine("<revision>959</revision>");
                outputWriter.WriteLine("</m_Version>");
                outputWriter.WriteLine("<m_ClassName text=\"" + materialClass + "\"/>");
                outputWriter.WriteLine("<m_DataFiles/>");
                outputWriter.WriteLine("<m_Name text=\"" + baseTextureMap + "\"/>");
                outputWriter.WriteLine("<m_Description text=\"\"/>");
                outputWriter.WriteLine("<m_Tags>");
                outputWriter.WriteLine("<Element text=\"" + materialClass + "\"/>");
                outputWriter.WriteLine("</m_Tags>");
                outputWriter.WriteLine("<m_Groups/>");
                outputWriter.WriteLine("</AssetObjects..MaterialInstance>");
                outputWriter.Close();
            }
        }

        public static void WriteGeometrySet(IGrannyFile file, string assetClassName)
        {
            string filenameNoExt = Path.GetFileNameWithoutExtension(file.Filename);
            string directory = Path.GetDirectoryName(file.Filename);
            string fileExtension = "_GeometrySet.xml";
            string geoFilename = filenameNoExt + fileExtension;

            StreamWriter outputWriter = new StreamWriter(new FileStream(directory + "\\" + geoFilename, FileMode.Create));
            List<IGrannyFile> files = new List<IGrannyFile>() {file};
            WriteGeometrySetMetadataToStream(files, outputWriter, null, assetClassName);
            outputWriter.Close();
        }

        public static void WriteBehaviorMetadata(string outputDirectory, Dictionary<string, string> civ6ShortNameToLongNameLookup, string dsgName)
        {
            StreamWriter outputWriter = new StreamWriter(new FileStream(outputDirectory + "\\m_BehaviorData.xml", FileMode.Create));
            WriteBehaviorMetadataToStream(civ6ShortNameToLongNameLookup, dsgName, outputWriter);
            outputWriter.Close();
        }

        private static void WriteGeometrySetMetadataToStream(List<IGrannyFile> files, StreamWriter outputWriter, List<Dictionary<string, string>> materialBindingToMtlDicts, string assetClassName)
        {
            List<string> stateNames = null;
            if (assetClassName.Equals("TileBase"))
            {
                stateNames = new List<string> { "Unworked", "Worked", "Pillaged", "Unbuilt", "Construction"};
            } else
            {
                stateNames = new List<string> {"Default"};
            }

            outputWriter.WriteLine("<m_GeometrySet>");
            outputWriter.WriteLine("<m_ModelInstances>");
            for (int i = 0; i < files.Count; i++)
            {
                IGrannyFile file = files[i];

                foreach (IGrannyModel model in file.Models)
                {
                    string filenameNoExt = Path.GetFileNameWithoutExtension(file.Filename);

                    string upperModelName = model.Name.ToUpper();
                    bool isTreeCut = upperModelName.Contains("TREE") && upperModelName.Contains("CUT");
                    
                    //First pass - check if any valid mesh bindings
                    bool modelHasValidMeshBinding = false;
                    foreach (IGrannyMesh mesh in model.MeshBindings)
                    {
                        string upperMeshName = mesh.Name.ToUpper();
                        bool isTreeCutMesh = upperMeshName.Contains("TREE") && upperMeshName.Contains("CUT");
                        bool isShadowMesh = upperMeshName.Contains("SHADOW");

                        if (!upperMeshName.Contains("DMG") && !isTreeCutMesh && !isShadowMesh)
                        {
                            modelHasValidMeshBinding = true;
                            break;
                        }
                    }

                    if (!isTreeCut && modelHasValidMeshBinding)
                    {
                        outputWriter.WriteLine("<Element>");
                        outputWriter.WriteLine("<m_Name text=\"" + model.Name + "\"/>");
                        outputWriter.WriteLine("<m_GeoName text=\"" + filenameNoExt + "\"/>");

                        outputWriter.WriteLine("<m_GroupStates>");

                        foreach (IGrannyMesh mesh in model.MeshBindings)
                        {
                            string upperMeshName = mesh.Name.ToUpper();
                            bool isTreeCutMesh = upperMeshName.Contains("TREE") && upperMeshName.Contains("CUT");
                            bool isShadowMesh = upperMeshName.Contains("SHADOW");

                            if (!upperMeshName.Contains("DMG") && !isTreeCutMesh && !isShadowMesh)
                            {
                                foreach (IGrannyTriMaterialGroup triMaterialGroup in mesh.TriangleMaterialGroups)
                                {
                                    string materialBindingName = mesh.MaterialBindings[triMaterialGroup.MaterialIndex].Name;

                                    foreach (string stateName in stateNames)
                                    {

                                        if (!materialBindingName.Contains("ColorMap") && !materialBindingName.Contains("Generic_Grey_8") && !materialBindingName.Contains("04 - Default"))
                                        {

                                            string mtlFilename = mesh.MaterialBindings[triMaterialGroup.MaterialIndex].Name;

                                            if (mtlFilename.Contains("."))
                                            {
                                                string[] parts = mtlFilename.Split('.');
                                                mtlFilename = parts[0];
                                            }

                                            outputWriter.WriteLine("<Element>");

                                            outputWriter.WriteLine("<m_Values>");
                                            outputWriter.WriteLine("<m_Values>");

                                            outputWriter.WriteLine("<Element class=\"AssetObjects:ObjectValue\">");

                                            string mtlFilenameToSet = mtlFilename;

                                            if (materialBindingToMtlDicts != null) { 
                                                Dictionary<string, string> materialBindingToMtlDict = materialBindingToMtlDicts[i];
                                                if (materialBindingToMtlDict != null)
                                                {
                                                    if (materialBindingToMtlDict.ContainsKey(materialBindingName))
                                                    {
                                                        mtlFilenameToSet = materialBindingToMtlDict[materialBindingName];
                                                    }
                                                }
                                            }

                                            outputWriter.WriteLine("<m_ObjectName text=\"" + mtlFilenameToSet + "\"/>");
                                            outputWriter.WriteLine("<m_eObjectType>MATERIAL</m_eObjectType>");
                                            outputWriter.WriteLine("<m_ParamName text=\"Material\"/>");
                                            outputWriter.WriteLine("</Element>");

                                            if (assetClassName.Equals("TileBase"))
                                            {
                                                outputWriter.WriteLine("<Element class=\"AssetObjects..BoolValue\">");
                                                outputWriter.WriteLine("<m_bValue>true</m_bValue>");
                                                outputWriter.WriteLine("<m_ParamName text=\"Visible\"/>");
                                                outputWriter.WriteLine("</Element>");
                                            }

                                            outputWriter.WriteLine("</m_Values>");
                                            outputWriter.WriteLine("</m_Values>");

                                            outputWriter.WriteLine("<m_GroupName text=\"" + materialBindingName + "\"/>");

                                            outputWriter.WriteLine("<m_MeshName text=\"" + mesh.Name + "\"/>");

                                            outputWriter.WriteLine("<m_StateName text=\"" + stateName + "\"/>");

                                            outputWriter.WriteLine("</Element>");
                                        }
                                    }
                                }
                            }
                        }
                        outputWriter.WriteLine("</m_GroupStates>");
                        outputWriter.WriteLine("</Element>");
                    }
                }
            }
            outputWriter.WriteLine("</m_ModelInstances>");
            outputWriter.WriteLine("</m_GeometrySet>");
        }

        private static void WriteBehaviorMetadataToStream(Dictionary<string, string> civ6ShortNameToLongNameLookup, string dsgName, StreamWriter outputWriter)
        {
            outputWriter.WriteLine("<m_BehaviorData>");
            outputWriter.WriteLine("<m_behaviorDataSets>");

            outputWriter.WriteLine("<m_animationBindings>");
            outputWriter.WriteLine("<m_Bindings>");

            foreach (string civ6AnimName in civ6ShortNameToLongNameLookup.Keys)
            {
                outputWriter.WriteLine("<Element>");
                outputWriter.WriteLine("<m_SlotName text=\"" + civ6AnimName + "\"/> ");
                outputWriter.WriteLine("<m_AnimationName text=\"" + civ6ShortNameToLongNameLookup[civ6AnimName] + "\"/> ");
                outputWriter.WriteLine("</Element>");
            }

            outputWriter.WriteLine("</m_Bindings>");
            outputWriter.WriteLine("</m_animationBindings>");

            outputWriter.WriteLine("<m_timelineBindings>");
            outputWriter.WriteLine("<m_Bindings>");

            foreach (string civ6AnimName in civ6ShortNameToLongNameLookup.Keys)
            {
                outputWriter.WriteLine("<Element>");
                outputWriter.WriteLine("<m_SlotName text=\"" + civ6AnimName + "\"/>");
                outputWriter.WriteLine("<m_TimelineName text=\"" + civ6AnimName + "\"/>");
                outputWriter.WriteLine("</Element>");
            }

            outputWriter.WriteLine("</m_Bindings>");
            outputWriter.WriteLine("</m_timelineBindings>");

            outputWriter.WriteLine("<m_timelines>");
            outputWriter.WriteLine("<m_Timelines>");

            foreach (string civ6AnimName in civ6ShortNameToLongNameLookup.Keys)
            {
                outputWriter.WriteLine("<Element>");
                outputWriter.WriteLine("<m_Name text=\"" + civ6AnimName + "\"/>");
                outputWriter.WriteLine("<m_Description text=\"\"/>");
                outputWriter.WriteLine("<m_AnimationName text=\"" + civ6ShortNameToLongNameLookup[civ6AnimName] + "\"/>");
                outputWriter.WriteLine("<m_fDuration>0.000000</m_fDuration>");
                outputWriter.WriteLine("<m_Triggers/>");
                outputWriter.WriteLine("</Element>");
            }

            outputWriter.WriteLine("</m_Timelines>");
            outputWriter.WriteLine("</m_timelines>");

            outputWriter.WriteLine("<m_attachmentPoints>");
            outputWriter.WriteLine("<m_Points>");
            outputWriter.WriteLine("</m_Points>");
            outputWriter.WriteLine("</m_attachmentPoints>");
            outputWriter.WriteLine("</m_behaviorDataSets>");
            outputWriter.WriteLine("<m_behaviorInstances/>");
            outputWriter.WriteLine("<m_dsgName text=\""+dsgName+"\"/>");
            outputWriter.WriteLine("<m_referenceGeometryNames/>");
            outputWriter.WriteLine("</m_BehaviorData>");
        }
    }
}
