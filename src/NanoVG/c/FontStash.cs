using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NanoVG
{
    static class FontStash
    {
        internal enum FONSflags
        {
            FONS_ZERO_TOPLEFT = 1,
            FONS_ZERO_BOTTOMLEFT = 2,
        }

        public struct FONSparams
        {
            public delegate int RenderCreate(object uptr, int width, int height);

            public delegate int RenderResize(object uptr, int width, int height);

            public delegate void RenderUpdate(object uptr, ref int rect, byte[] data);

            public delegate void RenderDraw(object uptr, Span<float> verts, Span<float> tcoords, Span<int> colors, int nverts);

            public delegate void RenderDelete(object uptr);

            public int width;
            public int height;
            public FONSflags flags;
            public object userPtr;
            public RenderCreate renderCreate;
            public RenderResize renderResize;
            public RenderUpdate renderUpdate;
            public RenderDraw renderDraw;
            public RenderDelete renderDelete;
        }

        public class FONScontext
        {

        }

        internal static FONScontext fonsCreateInternal(ref FONSparams @params)
        {
            /*
            FONScontext* stash = NULL;

            // Allocate memory for the font stash.
            stash = (FONScontext*)malloc(sizeof(FONScontext));
            if (stash == NULL) goto error;
            memset(stash, 0, sizeof(FONScontext));

            stash->params = *params;

            // Allocate scratch buffer.
            stash->scratch = (unsigned char*)malloc(FONS_SCRATCH_BUF_SIZE);
            if (stash->scratch == NULL) goto error;

            // Initialize implementation library
            if (!fons__tt_init(stash)) goto error;

            if (stash->params.renderCreate != NULL) {
                if (stash->params.renderCreate(stash->params.userPtr, stash->params.width, stash->params.height) == 0)
            goto error;
            }

            stash->atlas = fons__allocAtlas(stash->params.width, stash->params.height, FONS_INIT_ATLAS_NODES);
            if (stash->atlas == NULL) goto error;

            // Allocate space for fonts.
            stash->fonts = (FONSfont**)malloc(sizeof(FONSfont*) * FONS_INIT_FONTS);
            if (stash->fonts == NULL) goto error;
            memset(stash->fonts, 0, sizeof(FONSfont*) * FONS_INIT_FONTS);
            stash->cfonts = FONS_INIT_FONTS;
            stash->nfonts = 0;

            // Create texture for the cache.
            stash->itw = 1.0f / stash->params.width;
            stash->ith = 1.0f / stash->params.height;
            stash->texData = (unsigned char*)malloc(stash->params.width * stash->params.height);
            if (stash->texData == NULL) goto error;
            memset(stash->texData, 0, stash->params.width * stash->params.height);

            stash->dirtyRect[0] = stash->params.width;
            stash->dirtyRect[1] = stash->params.height;
            stash->dirtyRect[2] = 0;
            stash->dirtyRect[3] = 0;

            // Add white rect at 0,0 for debug drawing.
            fons__addWhiteRect(stash, 2, 2);

            fonsPushState(stash);
            fonsClearState(stash);

            return stash;

            error:
            fonsDeleteInternal(stash);
            return NULL;
            */
            return null;
        }

        internal static void fonsDeleteInternal(FONScontext stash)
        {
            /*
            int i;
            if (stash == NULL) return;

            if (stash->params.renderDelete)
                stash->params.renderDelete(stash->params.userPtr);

            for (i = 0; i < stash->nfonts; ++i)
                fons__freeFont(stash->fonts[i]);

            if (stash->atlas) fons__deleteAtlas(stash->atlas);
            if (stash->fonts) free(stash->fonts);
            if (stash->texData) free(stash->texData);
            if (stash->scratch) free(stash->scratch);
            free(stash);
            fons__tt_done(stash);
            */
        }
    }
}
