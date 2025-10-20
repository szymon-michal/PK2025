import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { 
  MessageCircle, 
  UserPlus, 
  Code, 
  GitFork, 
  Star,
  Clock
} from 'lucide-react';

interface Activity {
  id: number;
  type: 'message' | 'friend_request' | 'repository' | 'match';
  user: {
    name: string;
    avatar: string;
    nick: string;
  };
  content: string;
  timestamp: string;
  metadata?: any;
}

interface ActivityFeedProps {
  activities: Activity[];
}

export default function ActivityFeed({ activities }: ActivityFeedProps) {
  const getActivityIcon = (type: string) => {
    switch (type) {
      case 'message':
        return <MessageCircle className="h-4 w-4 text-blue-600" />;
      case 'friend_request':
        return <UserPlus className="h-4 w-4 text-green-600" />;
      case 'repository':
        return <Code className="h-4 w-4 text-purple-600" />;
      case 'match':
        return <Star className="h-4 w-4 text-orange-600" />;
      default:
        return <Clock className="h-4 w-4 text-gray-600" />;
    }
  };

  const getActivityBadge = (type: string) => {
    const badges = {
      message: { label: 'Message', variant: 'default' as const },
      friend_request: { label: 'Friend', variant: 'secondary' as const },
      repository: { label: 'Code', variant: 'outline' as const },
      match: { label: 'Match', variant: 'destructive' as const }
    };
    return badges[type as keyof typeof badges] || { label: 'Activity', variant: 'outline' as const };
  };

  return (
    <Card>
      <CardHeader>
        <CardTitle className="flex items-center space-x-2">
          <Clock className="h-5 w-5" />
          <span>Recent Activity</span>
        </CardTitle>
      </CardHeader>
      <CardContent>
        <div className="space-y-4">
          {activities.length === 0 ? (
            <div className="text-center py-8 text-muted-foreground">
              <Clock className="h-12 w-12 mx-auto mb-2 opacity-50" />
              <p>No recent activity</p>
              <p className="text-sm">Start connecting with other developers!</p>
            </div>
          ) : (
            activities.map((activity) => {
              const badge = getActivityBadge(activity.type);
              return (
                <div key={activity.id} className="flex items-start space-x-3 p-3 rounded-lg hover:bg-accent/50 transition-colors">
                  <Avatar className="h-8 w-8">
                    <AvatarImage src={activity.user.avatar} />
                    <AvatarFallback>{activity.user.name[0]}</AvatarFallback>
                  </Avatar>
                  
                  <div className="flex-1 min-w-0">
                    <div className="flex items-center space-x-2 mb-1">
                      <p className="text-sm font-medium truncate">
                        {activity.user.name}
                      </p>
                      <Badge variant={badge.variant} className="text-xs">
                        {badge.label}
                      </Badge>
                    </div>
                    
                    <p className="text-sm text-muted-foreground mb-2">
                      {activity.content}
                    </p>
                    
                    <div className="flex items-center justify-between">
                      <span className="text-xs text-muted-foreground">
                        {activity.timestamp}
                      </span>
                      
                      {activity.type === 'friend_request' && (
                        <div className="flex space-x-2">
                          <Button size="sm" variant="outline" className="h-6 px-2 text-xs">
                            Accept
                          </Button>
                          <Button size="sm" variant="ghost" className="h-6 px-2 text-xs">
                            Decline
                          </Button>
                        </div>
                      )}
                    </div>
                  </div>
                  
                  <div className="flex-shrink-0">
                    {getActivityIcon(activity.type)}
                  </div>
                </div>
              );
            })
          )}
        </div>
        
        {activities.length > 0 && (
          <div className="text-center mt-4">
            <Button variant="outline" size="sm">
              View All Activity
            </Button>
          </div>
        )}
      </CardContent>
    </Card>
  );
}