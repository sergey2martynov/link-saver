import * as signalR from '@microsoft/signalr'
import { onUnmounted } from 'vue'
import { useAuthStore } from '@/stores/auth'

// Connects to the LinkService SignalR hub and calls onLinkNamed whenever
// NamingService finishes generating a title for a link.
export function useLinksHub(onLinkNamed: (linkId: string, name: string) => void) {
  const auth = useAuthStore()

  // accessTokenFactory makes the SignalR client pass the JWT as ?access_token=...
  // in the WebSocket handshake (browsers can't set custom headers on WebSocket upgrades)
  const connection = new signalR.HubConnectionBuilder()
    .withUrl('/hubs/links', {
      accessTokenFactory: () => auth.token ?? ''
    })
    .withAutomaticReconnect()
    .build()

  connection.on('LinkNamed', onLinkNamed)
  connection.start().catch(err => console.error('SignalR connection failed:', err))

  onUnmounted(() => { connection.stop() })
}
