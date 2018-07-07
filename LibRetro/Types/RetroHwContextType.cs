namespace LibRetro.Types
{
    public enum RetroHwContextType
    {
        None = 0,

        /* OpenGL 2.x. Driver can choose to use latest compatibility context. */
        OpenGL = 1,

        /* OpenGL ES 2.0. */
        OpenGLES2 = 2,

        /* Modern desktop core GL context. Use version_major/
         * version_minor fields to set GL version. */
        OpenGLCore = 3,

        /* OpenGL ES 3.0 */
        OpenGLES3 = 4,

        /* OpenGL ES 3.1+. Set version_major/version_minor. For GLES2 and GLES3,
         * use the corresponding enums directly. */
        OpenGLESVersion = 5,

        /* Vulkan, see RETRO_ENVIRONMENT_GET_HW_RENDER_INTERFACE. */
        Vulkan = 6
    }
}