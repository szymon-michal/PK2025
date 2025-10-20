'use client';

import { useAuth } from '@/contexts/auth-context';
import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import StatsCards from '@/components/dashboard/stats-cards';
import ActivityFeed from '@/components/dashboard/activity-feed';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import { 
  Users, 
  MessageCircle, 
  Code, 
  Plus,
  Star,
  TrendingUp
} from 'lucide-react';

export default function DashboardPage() {
  const { user, loading } = useAuth();
  const router = useRouter();
  
  const [stats] = useState({
    totalFriends: 12,
    pendingRequests: 3,
    unreadMessages: 5,
    repositories: 8,
    matchScore: 0.85
  });

  const [activities] = useState([
    {
      id: 1,
      type: 'friend_request' as const,
      user: {
        name: 'Alice Johnson',
        avatar: 'https://api.dicebear.com/7.x/initials/svg?seed=Alice Johnson',
        nick: 'alice_dev'
      },
      content: 'sent you a friend request',
      timestamp: '2 minutes ago',
    },
    {
      id: 2,
      type: 'match' as const,
      user: {
        name: 'Bob Smith',
        avatar: 'https://api.dicebear.com/7.x/initials/svg?seed=Bob Smith',
        nick: 'bob_codes'
      },
      content: 'is a 87% match based on your skills',
      timestamp: '1 hour ago',
    },
    {
      id: 3,
      type: 'message' as const,
      user: {
        name: 'Sarah Wilson',
        avatar: 'https://api.dicebear.com/7.x/initials/svg?seed=Sarah Wilson',
        nick: 'sarah_react'
      },
      content: 'sent you a message about React best practices',
      timestamp: '3 hours ago',
    },
    {
      id: 4,
      type: 'repository' as const,
      user: {
        name: 'Mike Chen',
        avatar: 'https://api.dicebear.com/7.x/initials/svg?seed=Mike Chen',
        nick: 'mike_fullstack'
      },
      content: 'starred your repository "awesome-react-components"',
      timestamp: '1 day ago',
    }
  ]);

  const [quickActions] = useState([
    {
      title: 'Find Matches',
      description: 'Discover developers with similar interests',
      icon: Users,
      href: '/matches',
      color: 'bg-blue-500'
    },
    {
      title: 'Send Message',
      description: 'Start a conversation',
      icon: MessageCircle,
      href: '/messages',
      color: 'bg-green-500'
    },
    {
      title: 'Create Repository',
      description: 'Share your code with others',
      icon: Code,
      href: '/repositories',
      color: 'bg-purple-500'
    }
  ]);

  useEffect(() => {
    if (!loading && !user) {
      router.push('/login');
    }
  }, [user, loading, router]);

  if (loading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary"></div>
      </div>
    );
  }

  if (!user) return null;

  return (
    <div className="container mx-auto px-4 py-8">
      <div className="mb-8">
        <h1 className="text-3xl font-bold mb-2">Welcome back, {user.firstName}!</h1>
        <p className="text-muted-foreground">
          Here's what's happening in your developer network
        </p>
      </div>

      {/* Stats Cards */}
      <StatsCards stats={stats} />

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
        {/* Quick Actions */}
        <div className="lg:col-span-1">
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center space-x-2">
                <Star className="h-5 w-5" />
                <span>Quick Actions</span>
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="space-y-3">
                {quickActions.map((action, index) => (
                  <Button
                    key={index}
                    variant="outline"
                    className="w-full h-auto p-4 justify-start"
                    onClick={() => router.push(action.href)}
                  >
                    <div className="flex items-center space-x-3">
                      <div className={`p-2 rounded-lg ${action.color} text-white`}>
                        <action.icon className="h-4 w-4" />
                      </div>
                      <div className="text-left">
                        <div className="font-medium">{action.title}</div>
                        <div className="text-xs text-muted-foreground">
                          {action.description}
                        </div>
                      </div>
                    </div>
                  </Button>
                ))}
              </div>
            </CardContent>
          </Card>

          {/* Profile Summary */}
          <Card className="mt-6">
            <CardHeader>
              <CardTitle className="flex items-center space-x-2">
                <TrendingUp className="h-5 w-5" />
                <span>Your Profile</span>
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="flex items-center space-x-3 mb-4">
                <Avatar className="h-12 w-12">
                  <AvatarImage src={`https://api.dicebear.com/7.x/initials/svg?seed=${user.firstName} ${user.lastName}`} />
                  <AvatarFallback>
                    {user.firstName[0]}{user.lastName[0]}
                  </AvatarFallback>
                </Avatar>
                <div>
                  <p className="font-medium">{user.firstName} {user.lastName}</p>
                  <p className="text-sm text-muted-foreground">@{user.nick}</p>
                </div>
              </div>
              
              <div className="space-y-3">
                <div>
                  <p className="text-sm font-medium mb-2">Skills</p>
                  <div className="flex flex-wrap gap-1">
                    {user.skills.map((skill) => (
                      <Badge key={skill.id} variant="secondary" className="text-xs">
                        {skill.name}
                      </Badge>
                    ))}
                  </div>
                </div>
                
                <div>
                  <p className="text-sm font-medium mb-2">Interests</p>
                  <div className="flex flex-wrap gap-1">
                    {user.interests.map((interest) => (
                      <Badge key={interest.id} variant="outline" className="text-xs">
                        {interest.name}
                      </Badge>
                    ))}
                  </div>
                </div>
              </div>
              
              <Button variant="outline" size="sm" className="w-full mt-4">
                Edit Profile
              </Button>
            </CardContent>
          </Card>
        </div>

        {/* Activity Feed */}
        <div className="lg:col-span-2">
          <ActivityFeed activities={activities} />
        </div>
      </div>
    </div>
  );
}