import { Injectable } from '@angular/core';
import { marked } from 'marked';
import DOMPurify from 'dompurify';

@Injectable({
  providedIn: 'root'
})
export class MarkdownService {

  constructor() {
    // Configure marked options if needed
    marked.use({
      gfm: true,
      breaks: true
    });
  }

  /**
   * Parses markdown string to HTML and sanitizes it.
   * @param markdown The markdown string to parse.
   * @returns Sanitized HTML string.
   */
  public parse(markdown: string): string {
    if (!markdown) {
      return '';
    }

    const rawHtml = marked.parse(markdown) as string;
    return DOMPurify.sanitize(rawHtml);
  }
}
