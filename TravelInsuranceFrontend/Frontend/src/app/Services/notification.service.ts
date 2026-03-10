import { Injectable, signal } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { finalize, tap } from 'rxjs/operators';

export interface NotificationDTO {
    id: number;
    userId: string;
    message: string;
    isRead: boolean;
    createdAt: string;
    type: string;
}

@Injectable({
    providedIn: 'root'
})
export class NotificationService {
    private apiUrl = 'https://localhost:7161/api/Notification';

    // State
    public unreadCount = signal<number>(0);

    constructor(private http: HttpClient) { }

    private getHeaders(): HttpHeaders {
        const token = localStorage.getItem('token');
        return new HttpHeaders({
            'Authorization': `Bearer ${token}`
        });
    }

    getNotifications(): Observable<NotificationDTO[]> {
        return this.http.get<NotificationDTO[]>(this.apiUrl, { headers: this.getHeaders() }).pipe(
            tap(notifs => {
                const unread = notifs.filter(n => !n.isRead).length;
                this.unreadCount.set(unread);
            })
        );
    }

    markAsRead(id: number): Observable<any> {
        return this.http.put(`${this.apiUrl}/${id}/read`, {}, { headers: this.getHeaders() }).pipe(
            tap(() => {
                // Decrease unread count when marked as read
                const currentCount = this.unreadCount();
                if (currentCount > 0) {
                    this.unreadCount.set(currentCount - 1);
                }
            })
        );
    }
}
