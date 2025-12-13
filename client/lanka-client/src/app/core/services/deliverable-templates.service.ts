import { Injectable } from '@angular/core';
import { DeliverableFormat, IDeliverableTemplate } from '../models/campaigns';

@Injectable({
  providedIn: 'root'
})
export class DeliverableTemplatesService {
  private readonly templates: Record<DeliverableFormat, IDeliverableTemplate[]> = {
    [DeliverableFormat.InstagramPost]: [
      {
        format: DeliverableFormat.InstagramPost,
        description: 'Single Instagram feed post with professional photography',
        typicalRequirements: '1080x1080px or 1080x1350px image, product placement, brand mention in caption, 3-5 relevant hashtags',
        estimatedDuration: '2-3 hours'
      },
      {
        format: DeliverableFormat.InstagramPost,
        description: 'Instagram post with product review and detailed caption',
        typicalRequirements: 'High-quality image, honest review in caption (200-300 words), tag brand account, use branded hashtag',
        estimatedDuration: '3-4 hours'
      }
    ],
    [DeliverableFormat.InstagramReel]: [
      {
        format: DeliverableFormat.InstagramReel,
        description: '15-30 second Instagram Reel showcasing product features',
        typicalRequirements: 'Vertical video (1080x1920px), trending audio, clear product showcase, on-screen text, brand tag',
        estimatedDuration: '4-6 hours'
      },
      {
        format: DeliverableFormat.InstagramReel,
        description: '30-60 second Instagram Reel with unboxing and first impressions',
        typicalRequirements: 'Vertical video, authentic unboxing experience, voiceover or captions, product close-ups, brand mention',
        estimatedDuration: '5-7 hours'
      },
      {
        format: DeliverableFormat.InstagramReel,
        description: 'Tutorial-style Instagram Reel demonstrating product usage',
        typicalRequirements: 'Vertical video, step-by-step demonstration, clear instructions, engaging hook in first 3 seconds',
        estimatedDuration: '6-8 hours'
      }
    ],
    [DeliverableFormat.InstagramStory]: [
      {
        format: DeliverableFormat.InstagramStory,
        description: '3-5 Instagram Stories featuring product placement',
        typicalRequirements: 'Vertical format (1080x1920px), natural integration, swipe-up link (if available), brand tag, 24-hour visibility',
        estimatedDuration: '1-2 hours'
      },
      {
        format: DeliverableFormat.InstagramStory,
        description: 'Behind-the-scenes Instagram Story series (5-8 frames)',
        typicalRequirements: 'Authentic behind-the-scenes content, product integration, polls or questions for engagement, brand mention',
        estimatedDuration: '2-3 hours'
      }
    ],
    [DeliverableFormat.InstagramCarousel]: [
      {
        format: DeliverableFormat.InstagramCarousel,
        description: '5-10 slide Instagram carousel post with product showcase',
        typicalRequirements: 'Cohesive visual theme across all slides, detailed product features, consistent aspect ratio, informative captions',
        estimatedDuration: '4-5 hours'
      },
      {
        format: DeliverableFormat.InstagramCarousel,
        description: 'Before/After carousel showcasing product results',
        typicalRequirements: '3-5 slides, clear before/after comparisons, authentic results, detailed caption explaining process',
        estimatedDuration: '3-4 hours'
      }
    ],
    [DeliverableFormat.TikTokVideo]: [
      {
        format: DeliverableFormat.TikTokVideo,
        description: '15-60 second TikTok video with product integration',
        typicalRequirements: 'Vertical video (1080x1920px), trending sound/music, hashtag challenge participation, brand tag',
        estimatedDuration: '3-5 hours'
      },
      {
        format: DeliverableFormat.TikTokVideo,
        description: 'TikTok product review in trending video format',
        typicalRequirements: 'Authentic review, engaging hook, trending format usage, product demonstration, brand mention',
        estimatedDuration: '4-6 hours'
      }
    ],
    [DeliverableFormat.YouTubeVideo]: [
      {
        format: DeliverableFormat.YouTubeVideo,
        description: '5-10 minute YouTube product review video',
        typicalRequirements: '1080p or 4K video, detailed review, B-roll footage, custom thumbnail, SEO-optimized title/description, brand links in description',
        estimatedDuration: '10-15 hours'
      },
      {
        format: DeliverableFormat.YouTubeVideo,
        description: 'YouTube tutorial/how-to video featuring product',
        typicalRequirements: 'High-quality video production, step-by-step guide, clear voiceover, chapters/timestamps, product links',
        estimatedDuration: '12-20 hours'
      }
    ],
    [DeliverableFormat.YouTubeShort]: [
      {
        format: DeliverableFormat.YouTubeShort,
        description: '15-60 second YouTube Short with product showcase',
        typicalRequirements: 'Vertical video (9:16 aspect ratio), attention-grabbing opening, clear product feature, hashtags',
        estimatedDuration: '2-4 hours'
      }
    ],
    [DeliverableFormat.BlogPost]: [
      {
        format: DeliverableFormat.BlogPost,
        description: '800-1200 word blog post with product review',
        typicalRequirements: 'SEO-optimized content, 3-5 high-quality images, product pros/cons, personal experience, affiliate link integration',
        estimatedDuration: '5-7 hours'
      },
      {
        format: DeliverableFormat.BlogPost,
        description: 'Tutorial/guide blog post featuring product (1500-2000 words)',
        typicalRequirements: 'Detailed step-by-step guide, multiple images/screenshots, SEO optimization, internal/external links, product integration',
        estimatedDuration: '8-12 hours'
      }
    ],
    [DeliverableFormat.Custom]: [
      {
        format: DeliverableFormat.Custom,
        description: 'Custom deliverable - define your own specifications',
        typicalRequirements: 'Specify all requirements based on your unique offering',
        estimatedDuration: 'TBD'
      }
    ]
  };

  public getTemplatesForFormat(format: DeliverableFormat): IDeliverableTemplate[] {
    return this.templates[format] || [];
  }

  public getAllTemplates(): IDeliverableTemplate[] {
    return Object.values(this.templates).flat();
  }

  public getFormatOptions(): { label: string; value: DeliverableFormat }[] {
    return [
      { label: 'Instagram Post', value: DeliverableFormat.InstagramPost },
      { label: 'Instagram Reel', value: DeliverableFormat.InstagramReel },
      { label: 'Instagram Story', value: DeliverableFormat.InstagramStory },
      { label: 'Instagram Carousel', value: DeliverableFormat.InstagramCarousel },
      { label: 'TikTok Video', value: DeliverableFormat.TikTokVideo },
      { label: 'YouTube Video', value: DeliverableFormat.YouTubeVideo },
      { label: 'YouTube Short', value: DeliverableFormat.YouTubeShort },
      { label: 'Blog Post', value: DeliverableFormat.BlogPost },
      { label: 'Custom Format', value: DeliverableFormat.Custom }
    ];
  }

  public getFormatLabel(format: DeliverableFormat): string {
    return format;
  }

  public getFormatBadgeClass(format: DeliverableFormat): string {
    const classMap: Record<DeliverableFormat, string> = {
      [DeliverableFormat.InstagramPost]: 'format-instagram',
      [DeliverableFormat.InstagramReel]: 'format-instagram',
      [DeliverableFormat.InstagramStory]: 'format-instagram',
      [DeliverableFormat.InstagramCarousel]: 'format-instagram',
      [DeliverableFormat.TikTokVideo]: 'format-tiktok',
      [DeliverableFormat.YouTubeVideo]: 'format-youtube',
      [DeliverableFormat.YouTubeShort]: 'format-youtube',
      [DeliverableFormat.BlogPost]: 'format-blog',
      [DeliverableFormat.Custom]: 'format-custom'
    };
    return classMap[format] || 'format-default';
  }
}
