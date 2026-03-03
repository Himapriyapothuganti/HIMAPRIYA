import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
    providedIn: 'root'
})
export class AgentService {
    private apiUrl = 'https://localhost:7161/api/Agent';

    constructor(private http: HttpClient) { }

    /**
     * Retrieves the current user's token from local storage.
     */
    getToken(): string {
        return localStorage.getItem('token') || '';
    }

    /**
     * Generates authorization headers with the current token
     */
    private getHeaders(): HttpHeaders {
        return new HttpHeaders({
            'Authorization': `Bearer ${this.getToken()}`
        });
    }

    /**
     * Fetch aggregate data for the agent dashboard.
     */
    getDashboard(): Observable<any> {
        return this.http.get<any>(`${this.apiUrl}/dashboard`, { headers: this.getHeaders() });
    }

    /**
     * Fetch a list of all policies assigned to the current agent.
     */
    getMyPolicies(): Observable<any[]> {
        return this.http.get<any[]>(`${this.apiUrl}/policies`, { headers: this.getHeaders() });
    }

    /**
     * Fetch details for a specific policy.
     * @param id The ID of the policy
     */
    getPolicyById(id: number | string): Observable<any> {
        return this.http.get<any>(`${this.apiUrl}/policies/${id}`, { headers: this.getHeaders() });
    }
}
